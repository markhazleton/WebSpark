namespace WebSpark.Core.Interfaces;

public interface IBlogProvider
{
    Task<Models.BlogItem> GetBlog();
    Task<ICollection<Models.CategoryItem>> GetBlogCategories();
    Task<Models.BlogItem> GetBlogItem();
    Task<bool> Update(Models.BlogItem blog);
}
