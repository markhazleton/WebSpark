using System.Text;
using System.Text.Json;

namespace ConsoleApp1.Service;


public class OpenAiService
{
    private readonly HttpClient _httpClient;

    public OpenAiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T> GetChatResponseAsync<T>(object schemaModel) where T : class
    {
        // Build the JSON request body with the specified schema model
        var requestBody = new
        {
            model = "gpt-4o-2024-08-06",
            messages = new[]
            {
                new { role = "system", content = "You extract email addresses into JSON data." },
                new { role = "user", content = "Feeling stuck? Send a message to help@mycompany.com." }
            },
            response_format = new
            {
                type = "json_schema",
                json_schema = schemaModel
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            // Send the request
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            // Ensure response is successful
            response.EnsureSuccessStatusCode();

            // Deserialize response into the generic type T
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(jsonResponse);

            // Access and parse the content
            if (jsonDocument.RootElement.TryGetProperty("choices", out JsonElement choicesElement) &&
                choicesElement[0].TryGetProperty("message", out JsonElement messageElement) &&
                messageElement.TryGetProperty("content", out JsonElement contentElement))
            {
                // Deserialize the content string into type T
                return JsonSerializer.Deserialize<T>(contentElement.GetString());
            }
            else
            {
                Console.WriteLine("Unexpected response structure.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
        }

        return null;
    }
}
