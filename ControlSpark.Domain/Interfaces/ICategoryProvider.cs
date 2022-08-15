using ControlSpark.Domain.Entities;
using ControlSpark.Domain.Models;

namespace ControlSpark.Domain.Interfaces;

public interface ICategoryProvider
{
    Task<List<CategoryItem>> Categories();
    Task<List<CategoryItem>> SearchCategories(string term);

    Task<Category> GetCategory(int categoryId);
    Task<ICollection<Category>> GetPostCategories(int postId);

    Task<bool> SaveCategory(Category category);
    Task<Category> SaveCategory(string tag);

    Task<bool> AddPostCategory(int postId, string tag);
    Task<bool> SavePostCategories(int postId, List<Category> categories);

    Task<bool> RemoveCategory(int categoryId);
}
