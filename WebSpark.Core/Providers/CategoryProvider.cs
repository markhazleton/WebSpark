using Microsoft.Data.Sqlite;
using WebSpark.Core.Data;
using WebSpark.Domain.Interfaces;
using WebSpark.Domain.Models;

namespace WebSpark.Core.Providers;

public class CategoryProvider(WebSparkDbContext db) : ICategoryProvider, IDisposable
{
    public async Task<List<CategoryItem>> Categories()
    {
        var cats = new List<CategoryItem>();

        if (db.Posts != null && db.Posts.Any())
        {
            foreach (var pc in db.PostCategories.Include(pc => pc.Category).AsNoTracking())
            {
                if (!cats.Exists(c => c.Category.Equals(pc.Category.Content, StringComparison.CurrentCultureIgnoreCase)))
                {
                    cats.Add(new CategoryItem
                    {
                        Selected = false,
                        Id = pc.CategoryId,
                        Category = pc.Category.Content.ToLower(),
                        PostCount = 1,
                        DateCreated = pc.Category.DateCreated
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

    public async Task<List<CategoryItem>> SearchCategories(string term)
    {
        var cats = await Categories();

        if (term == "*")
            return cats;

        return cats.Where(c => c.Category.ToLower().Contains(term.ToLower())).ToList();
    }

    public async Task<CategoryItem> GetCategory(int categoryId)
    {
        return Create(await db.Categories.AsNoTracking()
            .Where(c => c.Id == categoryId)
            .FirstOrDefaultAsync());
    }

    public async Task<ICollection<CategoryItem>> GetPostCategories(int postId)
    {
        return Create(await db.PostCategories.AsNoTracking()
            .Where(pc => pc.PostId == postId)
            .Select(pc => pc.Category)
            .ToListAsync());
    }

    private static ICollection<CategoryItem> Create(List<Category> categories)
    {
        var cats = new List<CategoryItem>();
        foreach (var cat in categories)
        {
            cats.Add(new CategoryItem
            {
                Selected = false,
                Id = cat.Id,
                Category = cat.Content,
                Description = cat.Description,
                DateCreated = cat.DateCreated
            });
        }
        return cats;
    }

    public async Task<bool> SaveCategory(CategoryItem category)
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
        dbCategory.DateUpdated = DateTime.UtcNow;

        return await db.SaveChangesAsync() > 0;
    }

    private static CategoryItem Create(Category category)
    {
        return
            new()
            {
                Selected = false,
                Id = category.Id,
                Category = category.Content,
                Description = category.Description,
                DateCreated = category.DateCreated
            };
    }
    public async Task<CategoryItem> SaveCategory(string tag)
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
            DateCreated = DateTime.UtcNow
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

    private static Category Create(CategoryItem categoryItem)
    {
        return new Category
        {
            Id = categoryItem.Id,
            Content = categoryItem.Category,
            Description = categoryItem.Description,
            DateCreated = categoryItem.DateCreated
        };
    }

    public async Task<bool> SavePostCategories(int postId, List<CategoryItem> categories)
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
