using Microsoft.Data.Sqlite;
using WebSpark.Core.Data;

namespace WebSpark.Core.Providers;

public class CategoryProvider(WebSparkDbContext db) : Interfaces.ICategoryProvider, IDisposable
{
    public async Task<List<Models.CategoryItem>> Categories()
    {
        var cats = new List<Models.CategoryItem>();

        if (db.Posts != null && db.Posts.Any())
        {
            foreach (var pc in db.PostCategories.Include(pc => pc.Category).AsNoTracking())
            {
                if (!cats.Exists(c => c.Category.Equals(pc.Category.Content, StringComparison.CurrentCultureIgnoreCase)))
                {
                    cats.Add(new Models.CategoryItem
                    {
                        Selected = false,
                        Id = pc.CategoryId,
                        Category = pc.Category.Content.ToLower(),
                        PostCount = 1,
                        DateCreated = pc.Category.CreatedDate
                    });
                }
                else
                {
                    // update post count
                    var tmp = cats.Where(c => c.Category.Equals(pc.Category.Content, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    tmp.PostCount++;
                }
            }
        }
        return await Task.FromResult(cats);
    }

    public async Task<List<Models.CategoryItem>> SearchCategories(string term)
    {
        var cats = await Categories();

        if (term == "*")
            return cats;

        return cats.Where(c => c.Category.ToLower().Contains(term.ToLower())).ToList();
    }

    public async Task<Models.CategoryItem> GetCategory(int categoryId)
    {
        return Create(await db.Categories.AsNoTracking()
            .Where(c => c.Id == categoryId)
            .FirstOrDefaultAsync());
    }

    public async Task<ICollection<Models.CategoryItem>> GetPostCategories(int postId)
    {
        return Create(await db.PostCategories.AsNoTracking()
            .Where(pc => pc.PostId == postId)
            .Select(pc => pc.Category)
            .ToListAsync());
    }

    private static ICollection<Models.CategoryItem> Create(List<Category> categories)
    {
        var cats = new List<Models.CategoryItem>();
        foreach (var cat in categories)
        {
            cats.Add(new Models.CategoryItem
            {
                Selected = false,
                Id = cat.Id,
                Category = cat.Content,
                Description = cat.Description,
                DateCreated = cat.CreatedDate
            });
        }
        return cats;
    }

    public async Task<bool> SaveCategory(Models.CategoryItem category)
    {
        //Category existing = await _db.Categories.AsNoTracking()
        //    .Where(c => c.Content.ToLower() == category.Content.ToLower()).FirstOrDefaultAsync();

        //if (existing != null)
        //    return false; // already exists category with the same title

        Category dbCategory = await db.Categories.Where(c => c.Id == category.Id).FirstOrDefaultAsync();
        if (dbCategory == null)
            return false;

        dbCategory.Content = category.Category;
        dbCategory.Description = category.Description;
        dbCategory.UpdatedDate = DateTime.UtcNow;

        return await db.SaveChangesAsync() > 0;
    }

    private static Models.CategoryItem Create(Category category)
    {
        return
            new()
            {
                Selected = false,
                Id = category.Id,
                Category = category.Content,
                Description = category.Description,
                DateCreated = category.CreatedDate
            };
    }
    public async Task<Models.CategoryItem> SaveCategory(string tag)
    {
        Category category = await db.Categories
            .AsNoTracking()
            .Where(c => c.Content == tag)
            .FirstOrDefaultAsync();

        if (category != null)
            return Create(category);

        category = new Category()
        {
            Content = tag,
            CreatedDate = DateTime.UtcNow
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return Create(category);
    }

    public async Task<bool> AddPostCategory(int postId, string tag)
    {
        Category category = Create(await SaveCategory(tag));

        if (category == null)
            return false;

        Post post = await db.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
        if (post == null)
            return false;

        if (post.PostCategories == null)
            post.PostCategories = [];

        PostCategory postCategory = await db.PostCategories
            .AsNoTracking()
            .Where(pc => pc.CategoryId == category.Id)
            .Where(pc => pc.PostId == postId)
            .FirstOrDefaultAsync();

        if (postCategory == null)
        {
            db.PostCategories.Add(new PostCategory
            {
                CategoryId = category.Id,
                PostId = postId
            });
            return await db.SaveChangesAsync() > 0;
        }

        return false;
    }

    private static Category Create(Models.CategoryItem categoryItem)
    {
        return new Category
        {
            Id = categoryItem.Id,
            Content = categoryItem.Category,
            Description = categoryItem.Description,
            CreatedDate = categoryItem.DateCreated
        };
    }

    public async Task<bool> SavePostCategories(int postId, List<Models.CategoryItem> categories)
    {
        List<PostCategory> existingPostCategories = await db.PostCategories.AsNoTracking()
            .Where(pc => pc.PostId == postId).ToListAsync();

        db.PostCategories.RemoveRange(existingPostCategories);
        await db.SaveChangesAsync();
        foreach (var cat in categories)
        {
            await AddPostCategory(postId, cat.Category);
        }
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveCategory(int categoryId)
    {
        List<PostCategory> postCategories = await db.PostCategories
            .AsNoTracking()
            .Where(pc => pc.CategoryId == categoryId)
            .ToListAsync();
        db.PostCategories.RemoveRange(postCategories);

        Category category = db.Categories
                    .Where(c => c.Id == categoryId)
                    .FirstOrDefault();
        db.Categories.Remove(category);

        return await db.SaveChangesAsync() > 0;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        ((IDisposable)db).Dispose();
    }
}
