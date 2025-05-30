using WebSpark.Core.Models.ViewModels;

namespace WebSpark.Core.Interfaces;

/// <summary>
/// Recipe Service Interface
/// </summary>
public interface IRecipeService
{
    /// <summary>
    /// Deletes the specified identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    bool Delete(int Id);
    bool Delete(Models.RecipeCategoryModel saveItem);
    /// <summary>
    /// Gets the by identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>RecipeModel.</returns>
    Models.RecipeModel Get(int Id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    Models.RecipeCategoryModel GetRecipeCategoryById(int Id);

    /// <summary>
    /// Gets the recipe category list.
    /// </summary>
    /// <returns>List&lt;RecipeCategoryModel&gt;.</returns>
    List<Models.RecipeCategoryModel> GetRecipeCategoryList();

    /// <summary>
    /// Gets the recipe list.
    /// </summary>
    /// <returns>List&lt;RecipeModel&gt;.</returns>
    IEnumerable<Models.RecipeModel> Get();

    /// <summary>
    /// Saves the specified save item.
    /// </summary>
    /// <param name="saveItem">The save item.</param>
    /// <returns>RecipeModel.</returns>
    Models.RecipeModel Save(Models.RecipeModel saveItem);
    /// <summary>
    /// Get RecipeVM
    /// </summary>
    /// <param name="host"></param>
    /// <param name="baseVM"></param>
    /// <returns></returns>
    RecipeVM GetRecipeVMHostAsync(string host, WebsiteVM baseVM);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="saveItem"></param>
    /// <returns></returns>
    Models.RecipeCategoryModel Save(Models.RecipeCategoryModel saveItem);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    List<Models.RecipeImageModel> GetRecipeImages();
}
