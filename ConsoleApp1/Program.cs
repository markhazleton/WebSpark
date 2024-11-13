using ConsoleApp1.Service;
using System.Text;
using System.Text.Json;

// API Key and HttpClient setup
var openAiApiKey = "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";  // Replace with your actual API key
var options = new JsonSerializerOptions { WriteIndented = true };

using var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.openai.com/")
};
httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

// Define the JSON schema directly for a recipe structure
var recipeSchema = JsonSchemaGenerator.GenerateJsonSchema<RecipeData>(options);

var test = JsonSerializer.Deserialize<object>(recipeSchema);

// Create the request body with inline schema definition
var requestBody = new
{
    model = "gpt-4o",
    messages = new[]
    {
        new { role = "system", content = "You create a recipe JSON structure for a given dish." },
        new { role = "user", content = "Please generate a JSON for a recipe of Spaghetti Carbonara." }
    },
    response_format = new
    {
        type = "json_schema",
        json_schema = new
        {
            name = "recipe_schema",
            schema = test
        }
    }
};

var jsonRequest = JsonSerializer.Serialize(requestBody);
var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

try
{
    // Send the request
    var response = await httpClient.PostAsync("v1/chat/completions", content);

    // Ensure response is successful
    response.EnsureSuccessStatusCode();

    // Parse the response JSON and extract the content
    var jsonResponse = await response.Content.ReadAsStringAsync();
    var jsonDocument = JsonDocument.Parse(jsonResponse);

    // Check if the response has the 'choices' and 'message' elements
    if (jsonDocument.RootElement.TryGetProperty("choices", out JsonElement choicesElement) &&
        choicesElement[0].TryGetProperty("message", out JsonElement messageElement) &&
        messageElement.TryGetProperty("content", out JsonElement contentElement))
    {
        // Deserialize the content to RecipeData
        var recipeData = JsonSerializer.Deserialize<RecipeData>(contentElement.GetString());

        // Output RecipeData.Name to the console
        Console.WriteLine($"Recipe Name: {recipeData?.Name}");
    }
    else
    {
        Console.WriteLine("Unexpected response structure.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
