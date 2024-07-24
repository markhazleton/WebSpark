namespace WebSpark.Core.Interfaces;

public interface ICategoryProvider
{
    Task<List<Models.CategoryItem>> Categories();
    Task<List<Models.CategoryItem>> SearchCategories(string term);

    Task<Models.CategoryItem> GetCategory(int categoryId);
    Task<ICollection<Models.CategoryItem>> GetPostCategories(int postId);

    Task<bool> SaveCategory(Models.CategoryItem category);
    Task<Models.CategoryItem> SaveCategory(string tag);

    Task<bool> AddPostCategory(int postId, string tag);
    Task<bool> SavePostCategories(int postId, List<Models.CategoryItem> categories);

    Task<bool> RemoveCategory(int categoryId);
}
