using Microsoft.AspNetCore.Mvc.ViewEngines;
using Serilog;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using WebSpark.Core.Providers;
using WebSpark.Domain.Extensions;
using WebSpark.Domain.Interfaces;
using WebSpark.Domain.Models;
using WebSpark.Domain.ViewModels;

namespace WebSpark.Web.Controllers;
public class BlogController : BaseController
{
    protected readonly IBlogProvider _blogProvider;
    protected readonly IPostProvider _postProvider;
    protected readonly IFeedProvider _feedProvider;
    protected readonly IAuthorProvider _authorProvider;
    protected readonly IThemeProvider _themeProvider;
    protected readonly IStorageProvider _storageProvider;
    protected readonly ICompositeViewEngine _compositeViewEngine;

    public BlogController(IBlogProvider blogProvider,
        IPostProvider postProvider,
        IFeedProvider feedProvider,
        IAuthorProvider authorProvider,
        IThemeProvider themeProvider,
        IStorageProvider storageProvider,
        ICompositeViewEngine compositeViewEngine,
        ILogger<BlogController> logger,
        IConfiguration config,
        IWebsiteService websiteService)
        : base(logger, config, websiteService)
    {
        _blogProvider = blogProvider;
        _postProvider = postProvider;
        _feedProvider = feedProvider;
        _authorProvider = authorProvider;
        _themeProvider = themeProvider;
        _storageProvider = storageProvider;
        _compositeViewEngine = compositeViewEngine;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var blogVM = await getBlogPostsAsync(pager: page);
        //If no blogs are setup redirect to first time registration
        if (blogVM.Blog == null)
        {
            return Redirect("~/");
        }
        return View($"~/Views/Templates/{blogVM.Template}/Blog/Index.cshtml", blogVM);
    }

    //public async Task<IActionResult> Index(string slug)
    //{
    //    if (!string.IsNullOrEmpty(slug))
    //    {
    //        return await getSingleBlogPost(slug);
    //    }
    //    return Redirect("~/");
    //}


    [HttpPost]
    public async Task<IActionResult> Search(string term, int page = 1)
    {
        if (!string.IsNullOrEmpty(term))
        {
            var blogVM = await getBlogPostsAsync(pager: page);
            if (blogVM.Blog == null)
            {
                return Redirect("~/");
            }
            string viewPath = $"~/Views/Templates/{blogVM.Template}/Blog/Search.cshtml";
            if (IsViewExists(viewPath))
                return View(viewPath, blogVM);
            else
                return Redirect("~/home");
        }
        else
        {
            return Redirect("~/home");
        }
    }

    [HttpGet("categories/{id}")]
    public async Task<IActionResult> Categories(string id, int page = 1)
    {
        var blogVM = await getBlogPostsAsync(string.Empty, page, id);
        if (blogVM.Blog == null)
        {
            return Redirect("~/");
        }

        string viewPath = $"~/Views/Templates/{blogVM.Template}/Blog/Category.cshtml";

        ViewBag.Category = id;

        if (IsViewExists(viewPath))
            return View(viewPath, blogVM);

        return View($"~/Views/Templates/{blogVM.Template}/Blog/Index.cshtml", blogVM);
    }

    [HttpGet("posts/{slug}")]
    public async Task<IActionResult> Single(string slug)
    {
        return await getSingleBlogPost(slug);
    }

    [HttpGet("error")]
    public async Task<IActionResult> Error()
    {
        try
        {
            BlogVM model = new(BaseVM);
            model.Blog = await _blogProvider.GetBlogItem();
            string viewPath = $"~/Views/Templates/{model.Template}/Blog/404.cshtml";
            if (IsViewExists(viewPath))
                return View(viewPath, model);

            return View($"~/Views/Error.cshtml");
        }
        catch
        {
            return View($"~/Views/Error.cshtml");
        }
    }

    [ResponseCache(Duration = 1200)]
    // [HttpGet("feed/{type}")]
    public async Task<IActionResult> Rss(string type)
    {
        string host = Request.Scheme + "://" + Request.Host;
        var blog = await _blogProvider.GetBlog();

        var posts = await _feedProvider.GetEntries(type, host);
        var items = new List<SyndicationItem>();

        var feed = new SyndicationFeed(
             blog.Title,
             blog.Description,
             new Uri(host),
             host,
             posts.FirstOrDefault().Published
        );

        if (posts != null && posts.Count() > 0)
        {
            foreach (var post in posts)
            {
                var item = new SyndicationItem(
                     post.Title,
                     post.Description.MdToHtml(),
                     new Uri(post.Id),
                     post.Id,
                     post.Published
                );
                item.PublishDate = post.Published;
                items.Add(item);
            }
        }
        feed.Items = items;

        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Entitize,
            NewLineOnAttributes = true,
            Indent = true
        };

        using (var stream = new MemoryStream())
        {
            using (var xmlWriter = XmlWriter.Create(stream, settings))
            {
                var rssFormatter = new Rss20FeedFormatter(feed, false);
                rssFormatter.WriteTo(xmlWriter);
                xmlWriter.Flush();
            }
            return File(stream.ToArray(), "application/xml; charset=utf-8");
        }
    }

    private bool IsViewExists(string viewPath)
    {
        var result = _compositeViewEngine.GetView(string.Empty, viewPath, false);
        return result.Success;
    }


    public async Task<IActionResult> getSingleBlogPost(string slug)
    {
        try
        {
            BlogVM model = new(BaseVM);
            ViewBag.Slug = slug;

            PostModel postModel = await _postProvider.GetPostModel(slug);
            model.Older = postModel.Older;
            model.Newer = postModel.Newer;
            model.Post = postModel.Post;
            model.Related = postModel.Related;
            model.PostListType = PostListType.Blog;

            // If unpublished and unauthorized redirect to error / 404.
            //if (model.Post.Published == DateTime.MinValue && !User.Identity.IsAuthenticated)
            //{
            //    return Redirect("~/error");
            //}

            model.Blog = await _blogProvider.GetBlogItem();
            model.Post.Description = model.Post.Description.MdToHtml();
            model.Post.Content = model.Post.Content.MdToHtml();

            if (!model.Post.Author.Avatar.StartsWith("data:"))
                model.Post.Author.Avatar = Url.Content($"~/{model.Post.Author.Avatar}");

            if (model.Post.PostType == PostType.Page)
            {
                string viewPath = $"~/Views/Templates/{model.Template}/Blog/Page.cshtml";
                if (IsViewExists(viewPath))
                    return View(viewPath, model);
            }

            return View($"~/Views/Templates/{model.Template}/Blog/Post.cshtml", model);
        }
        catch
        {
            return Redirect("~/error");
        }
    }
    private async Task<BlogVM> getBlogPostsAsync(string term = "", int pager = 1, string category = "", string slug = "")
    {
        var model = new BlogVM(BaseVM);

        try
        {
            model.Blog = await _blogProvider.GetBlogItem();
            model.Pager = new Pager(pager, model.Blog.ItemsPerPage);

            if (!string.IsNullOrEmpty(category))
            {
                model.PostListType = PostListType.Category;
                model.Posts = await _postProvider.GetList(model.Pager, 0, category, "PF");
            }
            else if (string.IsNullOrEmpty(term))
            {
                model.PostListType = PostListType.Blog;
                if (model.Blog.IncludeFeatured)
                    model.Posts = await _postProvider.GetList(model.Pager, 0, string.Empty, "FP");
                else
                    model.Posts = await _postProvider.GetList(model.Pager, 0, string.Empty, "P");
            }
            else
            {
                model.PostListType = PostListType.Search;
                model.Blog.Title = term;
                model.Blog.Description = string.Empty;
                model.Posts = await _postProvider.Search(model.Pager, term, 0, "FP");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            Console.WriteLine(ex.Message);
            model.Blog = null;
            return model;
        }

        if (model.Pager.ShowOlder) model.Pager.LinkToOlder = $"?page={model.Pager.Older}";
        if (model.Pager.ShowNewer) model.Pager.LinkToNewer = $"?page={model.Pager.Newer}";

        if (!string.IsNullOrWhiteSpace(model?.Post?.Title))
        {
            model.PageTitle = model.Post.Title + " | " + model.WebsiteName;
            model.MetaDescription = model.Post.Description.StripHtml();
            model.PageCanonical = "/posts/" + model.Post.Slug;
        }
        return model;
    }
}
