using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptSpark.Domain.Service;

/// <summary>
/// Service class for generating recipe using GPT model.
/// </summary>
public class RecipeAzureAIOpenAIService : IRecipeGPTService
{
    private readonly OpenAIClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeAzureAIOpenAIService"/> class.
    /// </summary>
    /// <param name="configuration">The configuration object.</param>
    public RecipeAzureAIOpenAIService(IConfiguration configuration)
    {
        var OPENAI_API_KEY = configuration["OPENAI_API_KEY"];
        client = new OpenAIClient(OPENAI_API_KEY);
    }
    public class RecipeData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public List<string> Ingredients { get; set; } = new();
        public List<string> Instructions { get; set; } = new();
        public string Servings { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<WebSpark.Core.Models.RecipeModel> CreateMomGPTRecipe(string prompt, string category)
    {
        WebSpark.Core.Models.RecipeModel recipeModel = new();
        StringBuilder sb = new();
        sb.Append("You are MOM Recipe.  Write search engine optimized (SEO) friendly recipe for the Mechanics Of Motherhood web site recipe catalog.");
        sb.Append($"The recipe category is {category}. ");
        sb.Append("The description should be fun and search engine friendly and easy to follow. ");
        sb.Append("\"Provide a JSON formatted response with ALL of the following fields {\\r\\n  \\\"Name\\\": \\\"Please enter the name of the dish\\\",\\r\\n  \\\"Description\\\": \\\"Provide a short, enticing description of the dish in Markdown format. Use formatting such as **bold** for emphasis and *italics* for keywords to enhance SEO.\\\",\\r\\n  \\\"Category\\\": \\\"Specify the category of the recipe, such as 'Drink', 'Main Course', 'Vegetable, 'Side Dish', etc.\\\",\\r\\n  \\\"Ingredients\\\": \\\"List all ingredients required for the recipe including quantities as array of string.\\\",\\r\\n  \\\"Instructions\\\": \\\"List each step of the cooking process in an array of string.\\\",\\r\\n  \\\"Servings\\\": \\\"Indicate how many people this recipe serves.\\\",\\r\\n  \\\"SEO_Keywords\\\": \\\"Include array of strings of relevant SEO keywords here that will help boost the recipe's visibility in search engine results. Think about what a user might type into Google when looking for this recipe.\\\"\\r\\n}\\r\\n\"");
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Temperature = 0.7f,
            DeploymentName = "gpt-4-turbo",
            Messages =
            {
                new ChatRequestSystemMessage(sb.ToString()),
                new ChatRequestUserMessage(prompt)
            }
        };

        try
        {
            Response<ChatCompletions> responseGPT = await client.GetChatCompletionsAsync(chatCompletionsOptions);
            foreach (ChatChoice message in responseGPT.Value.Choices)
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        AllowTrailingCommas = true,
                        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement
                    };
                    var recipe = JsonSerializer.Deserialize<RecipeData>(message.Message.Content, options);
                    if (recipe != null && !string.IsNullOrWhiteSpace(recipe.Name))
                    {
                        // try cast recipe.servings to int
                        if (!int.TryParse(recipe.Servings, out int servings))
                        {
                            servings = 4;
                        }

                        recipeModel = new WebSpark.Core.Models.RecipeModel
                        {
                            Name = recipe.Name,
                            Description = recipe.Description,
                            Ingredients = recipe.Ingredients != null ? string.Join("\n- ", recipe.Ingredients.Prepend(string.Empty)) : string.Empty,
                            Instructions = recipe.Instructions != null ? string.Join("\n1. ", recipe.Instructions.Prepend(string.Empty)) : string.Empty,
                            Servings = servings,
                            AuthorNM = "MOM Recipe",
                            RecipeCategoryNM = recipe.Category,
                        };
                        break;
                    }
                }
                catch (JsonException)
                {
                    Console.WriteLine("Failed to parse JSON data; skipping invalid entry.");
                }
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
