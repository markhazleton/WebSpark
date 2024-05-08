using WebSpark.RecipeManager.Models;

namespace WebSpark.WebMvc.Service;

/// <summary>
/// RecipeAzureAIOpenAIService Interface
/// </summary>
public interface IRecipeGPTService
{
    /// <summary>
    /// Create a Recipe from a prompt
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="category"
    /// <returns></returns>
    Task<RecipeModel> CreateMomGPTRecipe(string prompt, string category);
}
