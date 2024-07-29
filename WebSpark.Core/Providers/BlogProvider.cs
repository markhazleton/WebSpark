using Microsoft.Data.Sqlite;
using WebSpark.Core.Data;

namespace WebSpark.Core.Providers;

public class BlogProvider : Interfaces.IBlogProvider, IDisposable
{
    private readonly WebSparkDbContext _db;
    private readonly IStorageProvider _storageProvider;

    public BlogProvider(WebSparkDbContext db, IStorageProvider storageProvider)
    {
        _db = db;
        _storageProvider = storageProvider;
    }

    public async Task<Models.BlogItem> GetBlogItem()
    {
        var blog = await _db.Blogs.AsNoTracking().OrderBy(b => b.Id).FirstAsync();
        blog.Theme = blog.Theme.ToLower();
        return new Models.BlogItem
        {
            Title = blog.Title,
            Description = blog.Description,
            Theme = blog.Theme,
            IncludeFeatured = blog.IncludeFeatured,
            ItemsPerPage = blog.ItemsPerPage,
            SocialFields = [],
            Cover = string.IsNullOrEmpty(blog.Cover) ? Models.Constants.DefaultCover : blog.Cover,
            Logo = string.IsNullOrEmpty(blog.Logo) ? Models.Constants.DefaultLogo : blog.Logo,
            HeaderScript = blog.HeaderScript,
            FooterScript = blog.FooterScript,
            values = await GetValues(blog.Theme)
        };
    }

    public async Task<Models.BlogItem> GetBlog()
    {
        return Create(await _db.Blogs.OrderBy(b => b.Id).AsNoTracking().FirstAsync());
    }

    private Models.BlogItem Create(Blog blog)
    {
        return new Models.BlogItem
        {
            Title = blog.Title,
            Description = blog.Description,
            Theme = blog.Theme,
            IncludeFeatured = blog.IncludeFeatured,
            ItemsPerPage = blog.ItemsPerPage,
            SocialFields = [],
            Cover = string.IsNullOrEmpty(blog.Cover) ? Models.Constants.DefaultCover : blog.Cover,
            Logo = string.IsNullOrEmpty(blog.Logo) ? Models.Constants.DefaultLogo : blog.Logo,
            HeaderScript = blog.HeaderScript,
            FooterScript = blog.FooterScript,
            values = GetValues(blog.Theme).Result
        };
    }

    public async Task<ICollection<Models.CategoryItem>> GetBlogCategories()
    {
        return Create(await _db.Categories.AsNoTracking().ToListAsync());
    }

    private static ICollection<Models.CategoryItem> Create(List<Category> categories)
    {
        return categories.Select(c => new Models.CategoryItem
        {
            Id = c.Id,
            Description = c.Description,
            DateCreated = c.CreatedDate,
            Selected = false,
            Category = c.Content,
        }).ToList();
    }

    public async Task<bool> Update(Models.BlogItem blogItem)
    {
        var blog = new Blog
        {
            Title = blogItem.Title,
            Description = blogItem.Description,
            ItemsPerPage = blogItem.ItemsPerPage,
            IncludeFeatured = blogItem.IncludeFeatured,
            Theme = blogItem.Theme,
            Cover = blogItem.Cover,
            Logo = blogItem.Logo,
            HeaderScript = blogItem.HeaderScript,
            FooterScript = blogItem.FooterScript
        };
        var existing = await _db.Blogs.OrderBy(b => b.Id).FirstAsync();

        existing.Title = blog.Title;
        existing.Description = blog.Description;
        existing.ItemsPerPage = blog.ItemsPerPage;
        existing.IncludeFeatured = blog.IncludeFeatured;
        existing.Theme = blog.Theme;
        existing.Cover = blog.Cover;
        existing.Logo = blog.Logo;
        existing.HeaderScript = blog.HeaderScript;
        existing.FooterScript = blog.FooterScript;
        existing.AnalyticsListType = blog.AnalyticsListType;
        existing.AnalyticsPeriod = blog.AnalyticsPeriod;

        return await _db.SaveChangesAsync() > 0;
    }

    private async Task<dynamic> GetValues(string theme)
    {
        var settings = await _storageProvider.GetThemeSettings(theme);
        var values = new Dictionary<string, string>();

        if (settings != null && settings.Sections != null)
        {
            foreach (var section in settings.Sections)
            {
                if (section.Fields != null)
                {
                    foreach (var field in section.Fields)
                    {
                        values.Add(field.Id, field.Value);
                    }
                }
            }
        }
        return values;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        ((IDisposable)_db).Dispose();
    }
}
