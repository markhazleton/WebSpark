using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PromptSpark.Domain.Data;
using PromptSpark.Domain.Models;
using PromptSpark.Domain.Models.OpenAI;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSpark.HttpClientUtility.RequestResult;

namespace PromptSpark.Domain.Service;

/// <summary>
/// Represents the service for GPT.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OpenAIChatCompletionService"/> class.
/// </remarks>
/// <param name="httpClientService">The HTTP client service.</param>
/// <param name="configuration">The configuration.</param>
/// <param name="context">The GPT database context.</param>
public class OpenAIChatCompletionService(
    IHttpRequestResultService httpClientService,
    IConfiguration configuration,
    GPTDbContext context,
    ILogger<OpenAIChatCompletionService> logger) : IGPTService
{
    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = false, // Prevents pretty-printing
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Excludes null values
    };

    private OpenAiApiRequestJson GetOpenAiApiRequestJson<T>(GPTDefinitionResponse definitionResponse, CancellationToken ct = default)
    {
        try
        {
            if (!double.TryParse(definitionResponse.Temperature, out double temperature))
                throw new Exception("temperature is not a valid double.");

            var systemMessage = new Message
            {
                role = "system",
                content = definitionResponse.SystemPrompt
            };

            var userMessage = new Message
            {
                role = "user",
                content = definitionResponse.UserPrompt
            };

            var messages = new List<Message> { systemMessage, userMessage };

            var openAiRequest = new OpenAiApiRequestJson
            {
                model = definitionResponse.Model,
                messages = messages,
                temperature = temperature
            };

            // Check if OutputType is "json" and add the JSON schema for T to the response_format
            if (definitionResponse.OutputType.ToString().Equals("json", StringComparison.CurrentCultureIgnoreCase))
            {
                var jsonSchema = JsonSchemaGenerator.GenerateJsonSchema<T>(options);
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonSchema);
                var stringJsonSchema = jsonElement.GetRawText();
                //openAiRequest.response_format = new ResponseFormat
                //{
                //    type = "json_object",
                //    JsonSchema = new Models.OpenAI.JsonSchema()
                //    {
                //        Name = "recipe_schema",
                //        Schema = stringJsonSchema
                //    }
                //};
            }
            return openAiRequest;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating OpenAI API request: {Message}", ex.Message);
        }
        return new OpenAiApiRequestJson();
    }


    private static OpenAiApiRequest GetOpenAiApiRequest(GPTDefinitionResponse definitionResponse, CancellationToken ct = default)
    {
        if (Double.TryParse(definitionResponse.Temperature, out double temperature) == false)
            throw new Exception("temperature is not a valid double.");

        var systemMessage = new Message()
        {
            role = "system",
            content = definitionResponse.SystemPrompt,
        };
        if (definitionResponse.OutputType.ToString().Equals("json", StringComparison.CurrentCultureIgnoreCase))
        {
            return new OpenAiApiRequest()
            {
                model = definitionResponse.Model,
                messages =
                    [
                        systemMessage,
                    new Message
                    {
                        role = "user",
                        content = definitionResponse.UserPrompt
                    }
                    ],
                temperature = temperature
            };
        }
        else
        {
            return new OpenAiApiRequest()
            {
                model = definitionResponse.Model,
                messages =
                    [
                        systemMessage,
                    new Message
                    {
                        role = "user",
                        content = definitionResponse.UserPrompt
                    }
                    ],
                temperature = temperature
            };
        }
    }

    public async Task<UserPromptDto> FindResponseByUserPromptTextAsync(string userPrompt)
    {
        try
        {
            var userPromptId = await context.Chats
                .Where(w => w.UserPrompt == userPrompt)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (userPromptId == 0)
                return new UserPromptDto();

            return await FindResponseByUserPromptIdAsync(userPromptId);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new UserPromptDto();

        }
    }
    public async Task<UserPromptDto> FindResponseByUserPromptIdAsync(int id)
    {
        // Attempt to retrieve the userPrompt from the database
        var response = await context.Chats
            .FirstOrDefaultAsync(r => r.Id == id);

        if (response == null)
            return new UserPromptDto();

        response.GPTResponses = await context.DefinitionResponses
            .Where(w => w.UserPrompt == response.UserPrompt &&
            w.DefinitionType == response.DefinitionType)
            .Include(i => i.Definition)
            .ToListAsync();

        // Return the userPrompt if found; otherwise, return null
        return response.ToDto();
    }


    public async Task<List<UserPromptDto>> GetUserPromptsByDefinitionTypeAsync(string definitionType)
    {
        return await context.Chats
            .Where(c => c.DefinitionType == definitionType)
            .Select(c => c.ToDto())
            .ToListAsync();
    }
    public async Task<UserPromptDto> RefreshUserPromptResponses(UserPromptDto userPrompt)
    {
        // Validate the UserPromtId exists
        var userPromptDto = await FindResponseByUserPromptIdAsync(userPrompt.UserPromptId);
        if (userPromptDto == null)
            return new();

        var dbUserPrompt = await context.Chats
            .Include(r => r.GPTResponses)
            .FirstOrDefaultAsync(r => r.Id == userPrompt.UserPromptId);

        // Get list of all Definitions
        var definitions = await context.Definitions
            .Where(w => w.DefinitionType == dbUserPrompt.DefinitionType)
            .ToListAsync();
        // loop over all the dbDefinitions and update the responses
        foreach (var definition in definitions)
        {
            // Find the GPTUserPrompt for the definition
            var definitionResponse = dbUserPrompt
                .GPTResponses
                .FirstOrDefault(r => r.DefinitionId == definition.DefinitionId);
            if (definitionResponse == null)
            {
                definitionResponse = new GPTDefinitionResponse(definition, dbUserPrompt);
                definitionResponse = await UpdateGPTResponse(definitionResponse);
                dbUserPrompt.GPTResponses.Add(definitionResponse);
            }
            else
            {
                if (definitionResponse.Model != definition.Model)
                    definitionResponse.Model = definition.Model;

                if (definitionResponse.UserPrompt != dbUserPrompt.UserPrompt)
                    definitionResponse.UserPrompt = dbUserPrompt.UserPrompt;

                if (definitionResponse.GPTName != definition.GPTName)
                    definitionResponse.GPTName = definition.GPTName;

                if (definitionResponse.Created < DateTime.Now.AddDays(-10))
                    definitionResponse.Created = DateTime.Now.AddDays(-5);

                definitionResponse = await UpdateGPTResponse(definitionResponse);
            }
        }
        context.Chats.Update(dbUserPrompt);
        await context.SaveChangesAsync();
        return dbUserPrompt.ToDto();
    }
    public async Task<UserPromptDto> RefreshGPTResponse(UserPromptDto userPrompt)
    {
        // Find the userPromptId to update
        var userPromptDto = await FindResponseByUserPromptTextAsync(userPrompt.UserPrompt);
        if (userPromptDto == null)
            return userPrompt;

        // get the user userPromptId from the database
        var dbUserPrompt = await context.Chats
            .Include(r => r.GPTResponses)
            .Where(w => w.DefinitionType == userPromptDto.DefinitionType)
            .FirstOrDefaultAsync(r => r.UserPrompt == userPrompt.UserPrompt);

        if (dbUserPrompt == null)
            return userPrompt;

        // Get list of all Definitions
        var dbDefinitions = await context.Definitions
            .Where(w => w.DefinitionType == userPromptDto.DefinitionType)
            .ToListAsync();

        // loop over all the dbDefinitions and update the responses
        foreach (var definition in dbDefinitions)
        {
            // Find the GPTUserPrompt for the definition
            var definitionResponse = dbUserPrompt.GPTResponses.FirstOrDefault(r => r.DefinitionId == definition.DefinitionId);
            if (definitionResponse == null)
            {
                definitionResponse = new GPTDefinitionResponse(definition, dbUserPrompt);
                definitionResponse = await UpdateGPTResponse(definitionResponse);
                dbUserPrompt.GPTResponses.Add(definitionResponse);
            }
            else
            {
                if (definitionResponse.UserPrompt != dbUserPrompt.UserPrompt)
                    definitionResponse.UserPrompt = dbUserPrompt.UserPrompt;

                if (definitionResponse.GPTName != definition.GPTName)
                    definitionResponse.GPTName = definition.GPTName;

                if (definitionResponse.Created < DateTime.Now.AddDays(-10))
                    definitionResponse.Created = DateTime.Now.AddDays(-5);

                if (definitionResponse.SystemResponse == "No Answer"
                    || definitionResponse.Updated.AddDays(1) < DateTime.Now)
                {
                    definitionResponse = await UpdateGPTResponse(definitionResponse);
                }
            }
        }
        try
        {
            context.Chats.Update(dbUserPrompt);
            await context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return userPrompt;
        }
        return dbUserPrompt.ToDto();
    }

    public async Task RerunAllPrompts()
    {
        var requests = await context.Chats
            .Select(c => c.ToDto())
            .ToListAsync();
        foreach (var req in requests)
        {
            await RefreshGPTResponse(req);
        }
    }
    public async Task RerunAllPrompts(int DefinitionId, CancellationToken ct = default)
    {
        // Filter chats to include only those that have at least one response matching the specified DefinitionId
        var userPrompts = await context.Chats
            .Where(c => c.GPTResponses.Any(r => r.DefinitionId == DefinitionId))  // Ensure there's a matching DefinitionId in DefinitionResponses
            .Include(c => c.GPTResponses).ThenInclude(r => r.Definition) // Include the DefinitionResponses and their associated Definitions
            .Select(c => c)
            .ToListAsync();

        foreach (var userPrompt in userPrompts)
        {
            foreach (var res in userPrompt.GPTResponses)
            {
                if (res.DefinitionId != DefinitionId)
                    continue;

                if (res.GPTName != res.Definition.GPTName)
                    res.GPTName = res.Definition.GPTName;

                if (res.UserPrompt != userPrompt.UserPrompt)
                    res.UserPrompt = userPrompt.UserPrompt;

                await UpdateGPTResponse(res);
                context.DefinitionResponses.Update(res);
                context.SaveChanges();
            }
        }
    }
    public async Task<GPTDefinitionResponse> UpdateGPTResponseJson<T>(GPTDefinitionResponse gptResponse, CancellationToken ct = default)
    {
        Dictionary<string, string> headers = new() { { "Authorization", $"Bearer {configuration.GetValue<string>("OPENAI_API_KEY") ?? "not found"}" } };
        string openAiUrl = configuration.GetValue<string>("OPENAI_URL") ?? "https://api.openai.com/v1/chat/completions";
        Uri openAiUri = new(openAiUrl);

        var openAIRequest = GetOpenAiApiRequestJson<T>(gptResponse);
        HttpRequestResult<OpenAiApiResponse> serviceResponse = new();

        // JsonSerializer options to control formatting and remove unnecessary whitespace
        // Serialize, trim, and create StringContent
        var serializedRequest = JsonSerializer.Serialize(openAIRequest, options).Trim();
        StringContent content = new(serializedRequest, Encoding.UTF8, "application/json");

        // Optional: Explicitly set the Content-Length if your HttpClientService requires it
        content.Headers.ContentLength = Encoding.UTF8.GetByteCount(serializedRequest);

        serviceResponse.RequestBody = content;
        serviceResponse.Retries = 0;
        serviceResponse.CacheDurationMinutes = 0;
        serviceResponse.RequestMethod = HttpMethod.Post;
        serviceResponse.RequestHeaders = headers;
        serviceResponse.RequestPath = openAiUrl.ToString();

        // Log serialized JSON for debugging if needed
        Console.WriteLine("Serialized Request JSON: " + serializedRequest);

        var response = await httpClientService.HttpSendRequestResultAsync(serviceResponse, ct: ct);
        try
        {

            gptResponse.DefinitionType = gptResponse.DefinitionType;
            gptResponse.SystemPrompt = openAIRequest.messages.Where(w => w.role == "system").FirstOrDefault()?.content;
            gptResponse.SystemResponse = serviceResponse?.ResponseResults?.Choices?.FirstOrDefault()?.Message?.content ?? "No Answer";
            gptResponse.Updated = serviceResponse?.CompletionDate ?? DateTime.Now;
            gptResponse.ElapsedMilliseconds = serviceResponse?.ElapsedMilliseconds ?? 0;
            gptResponse.TotalTokens = serviceResponse?.ResponseResults?.Usage?.TotalTokens ?? 0;
            gptResponse.CompletionTokens = serviceResponse?.ResponseResults?.Usage?.CompletionTokens ?? 0;
            gptResponse.PromptTokens = serviceResponse?.ResponseResults?.Usage?.PromptTokens ?? 0;
            gptResponse.Model = serviceResponse?.ResponseResults?.Model ?? "Unknown";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating GPT Response: {Message}", ex.Message);
        }
        return gptResponse;
    }

    public async Task<GPTDefinitionResponse> UpdateGPTResponse(GPTDefinitionResponse gptResponse, CancellationToken ct = default)
    {
        Dictionary<string, string> headers = new() { { "Authorization", $"Bearer {configuration.GetValue<string>("OPENAI_API_KEY") ?? "not found"}" } };
        string openAiUrl = configuration.GetValue<string>("OPENAI_URL") ?? "https://api.openai.com/v1/chat/completions";
        Uri openAiUri = new(openAiUrl);

        var openAIRequest = GetOpenAiApiRequest(gptResponse);
        HttpRequestResult<OpenAiApiResponse> serviceResponse = new();

        // JsonSerializer options to control formatting and remove unnecessary whitespace
        // Serialize, trim, and create StringContent
        var serializedRequest = JsonSerializer.Serialize(openAIRequest, options).Trim();
        StringContent content = new(serializedRequest, Encoding.UTF8, "application/json");

        // Optional: Explicitly set the Content-Length if your HttpClientService requires it
        content.Headers.ContentLength = Encoding.UTF8.GetByteCount(serializedRequest);

        serviceResponse.RequestBody = content;
        serviceResponse.Retries = 0;
        serviceResponse.CacheDurationMinutes = 0;
        serviceResponse.RequestMethod = HttpMethod.Post;
        serviceResponse.RequestHeaders = headers;
        serviceResponse.RequestPath = openAiUrl.ToString();

        // Log serialized JSON for debugging if needed
        Console.WriteLine("Serialized Request JSON: " + serializedRequest);

        var response = await httpClientService.HttpSendRequestResultAsync<OpenAiApiResponse>(serviceResponse, ct: ct);
        try
        {

            gptResponse.DefinitionType = gptResponse.DefinitionType;
            gptResponse.SystemPrompt = openAIRequest.messages.Where(w => w.role == "system").FirstOrDefault()?.content;
            gptResponse.SystemResponse = serviceResponse?.ResponseResults?.Choices?.FirstOrDefault()?.Message?.content ?? "No Answer";
            gptResponse.Updated = serviceResponse?.CompletionDate ?? DateTime.Now;
            gptResponse.ElapsedMilliseconds = serviceResponse?.ElapsedMilliseconds ?? 0;
            gptResponse.TotalTokens = serviceResponse?.ResponseResults?.Usage?.TotalTokens ?? 0;
            gptResponse.CompletionTokens = serviceResponse?.ResponseResults?.Usage?.CompletionTokens ?? 0;
            gptResponse.PromptTokens = serviceResponse?.ResponseResults?.Usage?.PromptTokens ?? 0;
            gptResponse.Model = serviceResponse?.ResponseResults?.Model ?? "Unknown";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating GPT Response: {Message}", ex.Message);
        }
        return gptResponse;
    }

}
public static class JsonSchemaGenerator
{
    public static string GenerateJsonSchema<T>(JsonSerializerOptions options, CancellationToken ct = default)
    {
        return GetSchemaSerialized<T>(options);
    }

    private static string GetSchemaSerialized<T>(JsonSerializerOptions options, CancellationToken ct = default)
    {
        var schema = new
        {
            type = "object",
            properties = GeneratePropertiesSchema(typeof(T)),
            additionalProperties = false
        };

        return JsonSerializer.Serialize(schema, options);
    }

    private static Dictionary<string, object> GeneratePropertiesSchema(Type type, CancellationToken ct = default)
    {
        var propertiesSchema = new Dictionary<string, object>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propertySchema = GetPropertySchema(property.PropertyType);
            propertiesSchema.Add(property.Name, propertySchema);
        }

        return propertiesSchema;
    }

    private static object GetPropertySchema(Type propertyType, CancellationToken ct = default)
    {
        if (propertyType == typeof(string))
        {
            return new { type = "string" };
        }
        else if (propertyType == typeof(int) || propertyType == typeof(long))
        {
            return new { type = "integer" };
        }
        else if (propertyType == typeof(double) || propertyType == typeof(float) || propertyType == typeof(decimal))
        {
            return new { type = "number" };
        }
        else if (propertyType == typeof(bool))
        {
            return new { type = "boolean" };
        }
        else if (typeof(IEnumerable<string>).IsAssignableFrom(propertyType))
        {
            return new
            {
                type = "array",
                items = new { type = "string" }
            };
        }
        else if (typeof(IEnumerable<int>).IsAssignableFrom(propertyType) || typeof(IEnumerable<long>).IsAssignableFrom(propertyType))
        {
            return new
            {
                type = "array",
                items = new { type = "integer" }
            };
        }
        else if (typeof(IEnumerable<double>).IsAssignableFrom(propertyType) || typeof(IEnumerable<float>).IsAssignableFrom(propertyType) || typeof(IEnumerable<decimal>).IsAssignableFrom(propertyType))
        {
            return new
            {
                type = "array",
                items = new { type = "number" }
            };
        }
        else if (propertyType.IsClass)
        {
            return new
            {
                type = "object",
                properties = GeneratePropertiesSchema(propertyType)
            };
        }
        else
        {
            return new { type = "string" }; // Default to string if type is unknown
        }
    }

    public static bool ValidateJsonAgainstSchema<T>(string json, JsonSerializerOptions options)
    {
        var schemaJson = GenerateJsonSchema<T>(options);
        var schemaDocument = JsonDocument.Parse(schemaJson);
        var jsonDocument = JsonDocument.Parse(json);

        // Get schema properties
        if (schemaDocument.RootElement.TryGetProperty("properties", out var schemaProperties))
        {
            return ValidateProperties(schemaProperties, jsonDocument.RootElement);
        }

        Console.WriteLine("Invalid schema format.");
        return false;
    }

    private static bool ValidateProperties(JsonElement schemaProperties, JsonElement jsonElement)
    {
        foreach (var schemaProperty in schemaProperties.EnumerateObject())
        {
            if (!jsonElement.TryGetProperty(schemaProperty.Name, out var jsonProperty))
            {
                Console.WriteLine($"Property '{schemaProperty.Name}' is missing in JSON.");
                return false;
            }

            var schemaType = schemaProperty.Value.GetProperty("type").GetString();
            if (!ValidatePropertyType(schemaType, jsonProperty))
            {
                Console.WriteLine($"Property '{schemaProperty.Name}' is of incorrect type. Expected: {schemaType}");
                return false;
            }
        }

        return true;
    }

    private static bool ValidatePropertyType(string schemaType, JsonElement jsonElement)
    {
        return schemaType switch
        {
            "string" => jsonElement.ValueKind == JsonValueKind.String,
            "integer" => jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out _),
            "number" => jsonElement.ValueKind == JsonValueKind.Number,
            "boolean" => jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False,
            "array" => jsonElement.ValueKind == JsonValueKind.Array,
            "object" => jsonElement.ValueKind == JsonValueKind.Object,
            _ => false
        };
    }
}