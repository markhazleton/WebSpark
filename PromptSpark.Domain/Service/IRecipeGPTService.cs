namespace PromptSpark.Domain.Service;

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
    Task<WebSpark.Core.Models.RecipeModel> CreateMomGPTRecipe(string prompt, string category);
}
