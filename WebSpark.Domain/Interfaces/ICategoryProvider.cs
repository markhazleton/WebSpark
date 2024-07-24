using WebSpark.Domain.Models;

namespace WebSpark.Domain.Interfaces;

public interface ICategoryProvider
{
    Task<List<CategoryItem>> Categories();
    Task<List<CategoryItem>> SearchCategories(string term);

    Task<CategoryItem> GetCategory(int categoryId);
    Task<ICollection<CategoryItem>> GetPostCategories(int postId);

    Task<bool> SaveCategory(CategoryItem category);
    Task<CategoryItem> SaveCategory(string tag);

    Task<bool> AddPostCategory(int postId, string tag);
    Task<bool> SavePostCategories(int postId, List<CategoryItem> categories);

    Task<bool> RemoveCategory(int categoryId);
}
