﻿using WebSpark.Core.Models;

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
    Task<RecipeModel> CreateMomGPTRecipe(RecipeModel model, string prompt, string category);
}
