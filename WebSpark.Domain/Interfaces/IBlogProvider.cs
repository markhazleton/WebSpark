using WebSpark.Domain.Models;

namespace WebSpark.Domain.Interfaces;

public interface IBlogProvider
{
    Task<BlogItem> GetBlog();
    Task<ICollection<CategoryItem>> GetBlogCategories();
    Task<BlogItem> GetBlogItem();
    Task<bool> Update(BlogItem blog);
}
