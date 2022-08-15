using ControlSpark.RecipeManager.Models;
using ControlSpark.RecipeManager.ViewModels;

namespace ControlSpark.RecipeManager.Interfaces;

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

    /// <summary>
    /// Gets the by identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>RecipeModel.</returns>
    RecipeModel Get(int Id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    RecipeCategoryModel GetRecipeCategoryById(int Id);

    /// <summary>
    /// Gets the recipe category list.
    /// </summary>
    /// <returns>List&lt;RecipeCategoryModel&gt;.</returns>
    List<RecipeCategoryModel> GetRecipeCategoryList();

    /// <summary>
    /// Gets the recipe list.
    /// </summary>
    /// <returns>List&lt;RecipeModel&gt;.</returns>
    IEnumerable<RecipeModel> Get();

    /// <summary>
    /// Saves the specified save item.
    /// </summary>
    /// <param name="saveItem">The save item.</param>
    /// <returns>RecipeModel.</returns>
    RecipeModel Save(RecipeModel saveItem);
    /// <summary>
    /// Get RecipeVM
    /// </summary>
    /// <param name="host"></param>
    /// <param name="defaultSiteId"></param>
    /// <param name="baseVM"></param>
    /// <returns></returns>
    RecipeVM GetRecipeVMHostAsync(string host, string defaultSiteId, WebsiteVM baseVM);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="saveItem"></param>
    /// <returns></returns>
    RecipeCategoryModel Save(RecipeCategoryModel saveItem);
}
