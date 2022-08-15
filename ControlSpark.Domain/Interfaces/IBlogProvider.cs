using ControlSpark.Domain.Entities;
using ControlSpark.Domain.Models;

namespace ControlSpark.Domain.Interfaces;

public interface IBlogProvider
{
    Task<Blog> GetBlog();
    Task<ICollection<Category>> GetBlogCategories();
    Task<BlogItem> GetBlogItem();
    Task<bool> Update(Blog blog);
}
