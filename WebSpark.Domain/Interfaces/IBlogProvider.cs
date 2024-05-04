using WebSpark.Domain.Entities;
using WebSpark.Domain.Models;

namespace WebSpark.Domain.Interfaces;

public interface IBlogProvider
{
    Task<Blog> GetBlog();
    Task<ICollection<Category>> GetBlogCategories();
    Task<BlogItem> GetBlogItem();
    Task<bool> Update(Blog blog);
}
