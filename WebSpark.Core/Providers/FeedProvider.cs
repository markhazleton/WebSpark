using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using WebSpark.Domain.Models;

namespace WebSpark.Core.Providers;

public interface IFeedProvider
{
    Task<IEnumerable<AtomEntry>> GetEntries(string type, string host);
}

public class FeedProvider : IFeedProvider
{
    protected readonly IPostProvider _postProvider;

    public FeedProvider(IPostProvider postProvider)
    {
        _postProvider = postProvider;
    }

    public async Task<IEnumerable<AtomEntry>> GetEntries(string type, string host)
    {
        var items = new List<AtomEntry>();
        var posts = await _postProvider.GetList(new Pager(1), 0, string.Empty, "P");

        foreach (var post in posts)
        {
            var item = new AtomEntry
            {
                Title = post.Title,
                Description = post.Content,
                Id = $"{host}/posts/{post.Slug}",
                Published = post.Published,
                LastUpdated = post.Published,
                ContentType = "html",
            };

            if (post.Categories != null && post.Categories.Count > 0)
            {
                foreach (CategoryItem category in post.Categories)
                {
                    item.AddCategory(new SyndicationCategory(category.Category));
                }
            }

            item.AddContributor(new SyndicationPerson(post.Author.Email, post.Author.DisplayName));
            item.AddLink(new SyndicationLink(new Uri(item.Id)));
            items.Add(item);
        }

        return await Task.FromResult(items);
    }
}
