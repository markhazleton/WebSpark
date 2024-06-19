using System.Text.Json;
using System.Text.Json.Serialization;
using PromptSpark.Domain.Data;
using PromptSpark.Domain.Service;
using WebSpark.RecipeManager.Models;

namespace WebSpark.WebMvc.Service;

/// <summary>
/// Initializes a new instance of the <see cref="RecipeAzureAIOpenAIService"/> class.
/// </summary>
/// <param name="_configuration">The _configuration object.</param>
public class RecipePromptSparkService(
    IConfiguration _configuration,
    IGPTDefinitionService _definitionService,
    IGPTService _promptService) : IRecipeGPTService
{
    public record RecipeData(
        string Name,
        string Description,
        string Category,
        int Servings,
        List<string> Ingredients = null,
        List<string> Instructions = null,
        List<string> SEO_Keywords = null
    )
    {
        public RecipeData() : this(
            Name: string.Empty,
            Description: string.Empty,
            Category: string.Empty,
            Ingredients: [],
            Instructions: [],
            Servings: 4,
            SEO_Keywords: []
        )
        { }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<RecipeModel> CreateMomGPTRecipe(string prompt, string category)
    {
        RecipeModel recipeModel = new();
        var recipeDefinition = await _definitionService.GetDefinitionDtoAsync(8);
        GPTDefinitionResponse defResponse = new()
        {
            DefinitionType = recipeDefinition.DefinitionType,
            GPTDescription = recipeDefinition.Description,
            SystemPrompt = recipeDefinition.Prompt,
            DefinitionId = recipeDefinition.DefinitionId,
            UserPrompt = prompt,
            Temperature = recipeDefinition.Temperature,
            Model = recipeDefinition.Model,
            OutputType = recipeDefinition.OutputType,
            GPTName = recipeDefinition.Name,
        };

        try
        {
            var response = await _promptService.UpdateGPTResponse(defResponse);

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    AllowTrailingCommas = true,
                    UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement
                };
                var recipe = JsonSerializer.Deserialize<RecipeData>(response.SystemResponse, options);
                if (recipe != null && !string.IsNullOrWhiteSpace(recipe.Name))
                {
                    recipeModel = new RecipeModel
                    {
                        Name = recipe.Name,
                        Description = recipe.Description,
                        Ingredients = recipe.Ingredients != null ? string.Join("\n- ", recipe.Ingredients.Prepend(string.Empty)) : string.Empty,
                        Instructions = recipe.Instructions != null ? string.Join("\n1. ", recipe.Instructions.Prepend(string.Empty)) : string.Empty,
                        SEO_Keywords = recipe.SEO_Keywords != null ? string.Join("\n1. ", recipe.SEO_Keywords.Prepend(string.Empty)) : string.Empty,
                        Servings = recipe.Servings,
                        AuthorNM = "MOM Recipe",
                        RecipeCategoryNM = recipe.Category,
                    };
                }
            }
            catch (JsonException)
            {
                Console.WriteLine("Failed to parse JSON data; skipping invalid entry.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }

        if (string.IsNullOrEmpty(recipeModel.Name))
        {
            throw new InvalidOperationException("No valid recipes were found.");
        }

        return recipeModel;
    }

}
