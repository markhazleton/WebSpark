using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PromptSpark.Domain.Data;
using System.Text;
using WebSpark.Core.Models;

namespace PromptSpark.Domain.Service;


/// <summary>
/// Initializes a new instance of the <see cref="RecipeAzureAIOpenAIService"/> class.
/// </summary>
/// <param name="configuration">The configuration object.</param>
public class RecipePromptSparkService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IGPTDefinitionService _definitionService,
    IGPTService _promptService) : IRecipeGPTService
{
    private static object CreateRequest(string userContent, string systemPrompt, object schema, double temperature = 0.8)
    {
        return new
        {
            model = "gpt-4o",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userContent }
            },
            temperature,
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "custom_model",
                    schema,
                    strict = true
                }
            }
        };
    }
    private string GetStringFromList(List<string> list)
    {
        StringBuilder sb = new();
        foreach (var item in list)
        {
            sb.Append("- ");
            sb.Append(item);
            sb.Append("\n");
        }
        return sb.ToString();
    }

    private static object MomRecipeSchema => new
    {
        type = "object",
        properties = new
        {
            Name = new { type = "string" },
            Description = new { type = "string" },
            Category = new { type = "string" },
            Ingredients = new { type = "array", items = new { type = "string" } },
            Instructions = new { type = "array", items = new { type = "string" } },
            Servings = new { type = "integer" },
            SEO_Keywords = new { type = "array", items = new { type = "string" } }
        },
        required = new[]
        {
                "Name", "Description", "Category", "Ingredients",
                "Instructions", "Servings", "SEO_Keywords"
            },
        additionalProperties = false
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
            var recipe = await GetMomRecipeAIAsync(defResponse);
            try
            {
                if (recipe != null && !string.IsNullOrWhiteSpace(recipe.Name))
                {
                    recipeModel.LastViewDT = DateTime.Now;
                    recipeModel.ModifiedDT = DateTime.Now;
                    recipeModel.Name = recipe.Name;
                    recipeModel.Description = recipe.Description;
                    recipeModel.Ingredients = recipe.Ingredients;
                    recipeModel.Instructions = recipe.Instructions;
                    recipeModel.Servings = recipe.Servings;
                    recipeModel.SEO_Keywords = recipe.SEO_Keywords;
                    recipeModel.IsApproved = true;
                    recipeModel.AverageRating = 5;
                    recipeModel.AuthorNM = "MOM Recipe";
                    recipeModel.RecipeCategoryNM = category;
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

    public async Task<RecipeModel> GetMomRecipeAIAsync(GPTDefinitionResponse defResponse)
    {
        string apikey = configuration.GetValue<string>("OPENAI_API_KEY") ?? "not found";
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {apikey}");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Accept-Language", "en-US,en; q=0.9");
        var requestPayload = CreateRequest(defResponse.UserPrompt, defResponse.SystemPrompt, MomRecipeSchema);
        request.Content = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8, "application/json");
        HttpClient httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var chatCompletionResponse = JsonConvert.DeserializeObject<ChatCompletionResponse>(responseContent);
        RecipeModelAI recipeAI = JsonConvert.DeserializeObject<RecipeModelAI>(chatCompletionResponse.Choices[0].Message.Content);


        

        RecipeModel recipe = new()
        {
            Name = recipeAI.Name,
            Description = recipeAI.Description,
            Ingredients = GetStringFromList(recipeAI.Ingredients),
            Instructions = GetStringFromList(recipeAI.Instructions),
            Servings = recipeAI.Servings,
            SEO_Keywords = GetStringFromList(recipeAI.SeoKeywords),
            AuthorNM = "MOM Recipe",
            AverageRating = 5,
        };

        return recipe;
    }

    private class RecipeModelAI
    {

        [JsonProperty("Category")]
        public string Category { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Ingredients")]
        public List<string> Ingredients { get; set; }

        [JsonProperty("Instructions")]
        public List<string> Instructions { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("SEO_Keywords")]
        public List<string> SeoKeywords { get; set; }

        [JsonProperty("Servings")]
        public int Servings { get; set; }
    }
    private class ChatCompletionResponse
    {

        [JsonProperty("choices")]
        public List<Choice> Choices { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("system_fingerprint")]
        public string SystemFingerprint { get; set; }

        [JsonProperty("usage")]
        public Usage Usage { get; set; }
    }
    private class Choice
    {

        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("logprobs")]
        public object LogProbs { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }
    }
    private class Message
    {

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("refusal")]
        public object Refusal { get; set; }
        [JsonProperty("role")]
        public string Role { get; set; }
    }
    private class Usage
    {

        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("completion_tokens_details")]
        public TokenDetails CompletionTokensDetails { get; set; }
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("prompt_tokens_details")]
        public TokenDetails PromptTokensDetails { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }
    private class TokenDetails
    {

        [JsonProperty("accepted_prediction_tokens")]
        public int AcceptedPredictionTokens { get; set; }

        [JsonProperty("audio_tokens")]
        public int AudioTokens { get; set; }
        [JsonProperty("cached_tokens")]
        public int CachedTokens { get; set; }

        [JsonProperty("reasoning_tokens")]
        public int ReasoningTokens { get; set; }

        [JsonProperty("rejected_prediction_tokens")]
        public int RejectedPredictionTokens { get; set; }
    }

}




