using Serilog;
using System;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using WebSpark.Core.Data;
using WebSpark.Core.Extensions;

namespace WebSpark.Core.Providers
{
    public interface ISyndicationProvider
    {
        Task<List<Post>> GetPosts(string feedUrl, int userId, Uri baseUrl, string webRoot = "/");
        Task<bool> ImportPost(Post post);
    }

    public class SyndicationProvider : ISyndicationProvider
    {
        private readonly WebSparkDbContext _dbContext;
        private readonly IStorageProvider _storageProvider;

        private static int _userId;
        private static string _webRoot;
        private static Uri _baseUrl;

        public SyndicationProvider(WebSparkDbContext dbContext, IStorageProvider storageProvider)
        {
            _dbContext = dbContext;
            _storageProvider = storageProvider;
        }

        public async Task<List<Post>> GetPosts(string feedUrl, int userId, Uri baseUrl, string webRoot = "/")
        {
            _userId = userId;
            _webRoot = webRoot;
            _baseUrl = baseUrl;

            List<Post> posts = [];
            try
            {
                SyndicationFeed feed = SyndicationFeed.Load(XmlReader.Create(feedUrl));
                foreach (var item in feed.Items)
                {
                    posts.Add(await GetPost(item));
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error parsing feed URL: {ex.Message}");
            }
            return posts;
        }

        public async Task<bool> ImportPost(Post post)
        {
            try
            {
                await ImportImages(post);
                await ImportFiles(post);

                var converter = new ReverseMarkdown.Converter();

                post.Description = GetDescription(converter.Convert(post.Description));
                post.Content = converter.Convert(post.Content);
                post.Selected = false;

                await _dbContext.Posts.AddAsync(post);
                if (await _dbContext.SaveChangesAsync() == 0)
                {
                    Log.Error($"Error saving post {post.Title}");
                    return false;
                }

                Post savedPost = await _dbContext.Posts.SingleAsync(p => p.Slug == post.Slug);
                if (savedPost == null)
                {
                    Log.Error($"Error finding saved post - {post.Title}");
                    return false;
                }

                savedPost.Blog = await _dbContext.Blogs.FirstOrDefaultAsync();
                return await _dbContext.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Log.Error($"Error importing post {post.Title}: {ex.Message}");
                return false;
            }
        }


        async Task<Post> GetPost(SyndicationItem syndicationItem)
        {
            Post post = new()
            {
                AuthorId = _userId,
                PostType = Models.PostType.Post,
                Title = syndicationItem.Title.Text,
                Slug = await GetSlug(syndicationItem.Title.Text),
                Description = GetDescription(syndicationItem.Title.Text),
                Content = syndicationItem.Summary.Text,
                Cover = Models.Constants.DefaultCover,
                Published = syndicationItem.PublishDate.DateTime,
                CreatedDate = syndicationItem.PublishDate.DateTime,
                UpdatedDate = syndicationItem.LastUpdatedTime.DateTime
            };

            if (syndicationItem.ElementExtensions != null)
            {
                foreach (SyndicationElementExtension ext in syndicationItem.ElementExtensions)
                {
                    if (ext.GetObject<XElement>().Name.LocalName == "summary")
                        post.Description = GetDescription(ext.GetObject<XElement>().Value);

                    if (ext.GetObject<XElement>().Name.LocalName == "cover")
                        post.Cover = ext.GetObject<XElement>().Value;
                }
            }

            if (syndicationItem.Categories != null)
            {
                if (post.PostCategories == null)
                    post.PostCategories = [];

                foreach (var category in syndicationItem.Categories)
                {
                    post.PostCategories.Add(new PostCategory()
                    {
                        Category = new Category
                        {
                            Content = category.Name,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow
                        }
                    });
                }
            }

            return post;
        }

        async Task ImportImages(Post post)
        {
            string rgx = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";

            if (string.IsNullOrEmpty(post.Content))
                return;

            if (post.Cover != Models.Constants.DefaultCover)
            {
                var path = $"{post.AuthorId}/{post.Published.Year}/{post.Published.Month}";

                var mdTag = await _storageProvider.UploadFromWeb(new Uri(post.Cover), _webRoot, path);
                if (mdTag.Length > 0 && mdTag.IndexOf("(") > 2)
                    post.Cover = mdTag.Substring(mdTag.IndexOf("(") + 2).Replace(")", string.Empty);
            }

            var matches = Regex.Matches(post.Content, rgx, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (matches == null)
                return;

            foreach (Match m in matches)
            {
                try
                {
                    var tag = m.Groups[0].Value;
                    var path = $"{post.AuthorId}/{post.Published.Year}/{post.Published.Month}";

                    var uri = Regex.Match(tag, "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1].Value;
                    uri = ValidateUrl(uri);
                    var mdTag = string.Empty;

                    if (uri.Contains("data:image"))
                        mdTag = await _storageProvider.UploadBase64Image(uri, _webRoot, path);
                    else
                    {
                        try
                        {
                            mdTag = await _storageProvider.UploadFromWeb(new Uri(uri), _webRoot, path);
                        }
                        catch
                        {
                            if (uri.StartsWith("https:"))
                            {
                                mdTag = await _storageProvider.UploadFromWeb(new Uri(uri.Replace("https:", "http:")), _webRoot, path);
                            }
                        }
                    }
                    post.Content = post.Content.ReplaceIgnoreCase(tag, mdTag);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error importing images: {ex.Message}");
                }
            }
        }

        async Task ImportFiles(Post post)
        {
            var rgx = @"(?i)<a\b[^>]*?>(?<text>.*?)</a>";
            string[] exts = ["zip", "7z", "xml", "pdf", "doc", "docx", "xls", "xlsx", "mp3", "mp4", "avi"];

            if (string.IsNullOrEmpty(post.Content))
                return;

            MatchCollection matches = Regex.Matches(post.Content, rgx, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (matches != null)
            {
                foreach (Match m in matches)
                {
                    try
                    {
                        var tag = m.Value;
                        var src = XElement.Parse(tag).Attribute("href").Value;
                        var mdTag = string.Empty;

                        foreach (var ext in exts)
                        {
                            if (src.ToLower().EndsWith($".{ext}"))
                            {
                                var uri = ValidateUrl(src);
                                var path = $"{post.AuthorId}/{post.Published.Year}/{post.Published.Month}";

                                mdTag = await _storageProvider.UploadFromWeb(new Uri(uri), _webRoot, path);

                                if (mdTag.StartsWith("!"))
                                    mdTag = mdTag.Substring(1);

                                post.Content = post.Content.ReplaceIgnoreCase(m.Value, mdTag);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error importing files: {ex.Message}");
                    }
                }
            }
        }

        async Task<string> GetSlug(string title)
        {
            string slug = title.ToSlug();
            Post post = await _dbContext.Posts.SingleOrDefaultAsync(p => p.Slug == slug);

            if (post == null)
                return slug;

            for (int i = 2; i < 100; i++)
            {
                post = await _dbContext.Posts.AsNoTracking()
                    .SingleAsync(p => p.Slug == $"{slug}{i}");

                if (post == null)
                    return await Task.FromResult($"{slug}{i}");
            }
            return slug;
        }

        static string ValidateUrl(string link)
        {
            var url = link;

            var baseUrl = _baseUrl.ToString();
            if (baseUrl.EndsWith("/"))
                baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);

            if (url.StartsWith("~"))
                url = url.Replace("~", baseUrl);

            if (url.StartsWith("/"))
                url = $"{baseUrl}{url}";

            if (!(url.StartsWith("http:") || url.StartsWith("https:")))
                url = $"{baseUrl}/{url}";

            return url;
        }

        static string GetDescription(string description)
        {
            description = description.StripHtml();
            if (description.Length > 450)
                description = $"{(description.Substring(0, 446))}...";
            return description;
        }

    }
}
