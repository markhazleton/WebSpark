using Microsoft.Extensions.Configuration;
using PromptSpark.Domain.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using WebSpark.Core.Models;

namespace PromptSpark.Domain.Service;
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
/// Initializes a new instance of the <see cref="RecipeAzureAIOpenAIService"/> class.
/// </summary>
/// <param name="_configuration">The _configuration object.</param>
public class RecipePromptSparkService(
    IConfiguration _configuration,
    IGPTDefinitionService _definitionService,
    IGPTService _promptService) : IRecipeGPTService
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters = { new IntOrStringConverter() },
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        AllowTrailingCommas = true,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<RecipeModel> CreateMomGPTRecipe(RecipeModel recipeModel, string prompt, string category)
    {

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
            var response = await _promptService.UpdateGPTResponseJson<RecipeData>(defResponse);
            try
            {
                var recipe = JsonHelper.RetrieveAndValidateJson<RecipeData>(response.SystemResponse, _jsonSerializerOptions);

                if (recipe != null && !string.IsNullOrWhiteSpace(recipe.Name))
                {
                    recipeModel.Name = recipe.Name;
                    recipeModel.Description = recipe.Description;
                    recipeModel.Ingredients = recipe.Ingredients != null ? string.Join("\n- ", recipe.Ingredients.Prepend(string.Empty)) : string.Empty;
                    recipeModel.Instructions = recipe.Instructions != null ? string.Join("\n1. ", recipe.Instructions.Prepend(string.Empty)) : string.Empty;
                    recipeModel.SEO_Keywords = recipe.SEO_Keywords != null ? string.Join("\n1. ", recipe.SEO_Keywords.Prepend(string.Empty)) : string.Empty;
                    recipeModel.Servings = recipe.Servings;
                    recipeModel.AuthorNM = "MOM Recipe";
                    recipeModel.RecipeCategoryNM = recipe.Category;
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
public static class JsonHelper
{
    public static T RetrieveAndValidateJson<T>(string content, JsonSerializerOptions options)
    {
        // Step 1: Check if content is valid JSON
        if (IsValidJson(content))
        {
            return JsonSerializer.Deserialize<T>(content,options)!; // Deserialize and return
        }

        // Step 2: Check if it starts with ```json and ends with ```
        var codeBlockPattern = @"^```json\s*([\s\S]*?)\s*```$";
        var match = Regex.Match(content, codeBlockPattern, RegexOptions.IgnoreCase);

        if (match.Success)
        {
            // Extract the JSON part from the code block
            content = match.Groups[1].Value;
        }

        // Step 3: Clean content - Remove special characters, whitespace, and line returns
        content = Regex.Replace(content, @"\s+", ""); // Removes all whitespaces and line returns

        // Step 4: Check cleaned content for valid JSON
        if (IsValidJson(content))
        {
            return JsonSerializer.Deserialize<T>(content,options)!; // Deserialize and return
        }

        throw new InvalidDataException("The content is not valid JSON.");
    }

    private static bool IsValidJson(string content)
    {
        try
        {
            // Attempt to parse as JSON
            JsonDocument.Parse(content);
            return true;
        }
        catch (JsonException)
        {
            return false; // If parsing fails, it's not valid JSON
        }
    }
}

public class IntOrStringConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int intValue))
        {
            return intValue;
        }
        else if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out intValue))
        {
            return intValue;
        }

        // Default to 0 if the value cannot be parsed as an integer
        return 0;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value); // Write as integer
    }
}

