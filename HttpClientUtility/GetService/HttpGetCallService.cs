using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpClientUtility.GetService;

/// <summary>
/// Represents the service for making HTTP GET calls.
/// </summary>
/// <param name="logger"></param>
/// <param name="httpClientFactory"></param>
public class HttpGetCallService(
    ILogger<HttpGetCallService> logger, 
    IHttpClientFactory httpClientFactory) : IHttpGetCallService
{

    // Define relaxed JSON options
    private static readonly JsonSerializerOptions _relaxedJsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true, // Ignore case for property names
        AllowTrailingCommas = true,         // Allow trailing commas in JSON
        ReadCommentHandling = JsonCommentHandling.Skip, // Skip comments in JSON
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Ignore null values when serializing
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString, // Handle numbers as strings
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement // Allow deserialization of unknown types as JsonElement
    };

    /// <summary>
    /// GetAsync
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="statusCall"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<HttpGetCallResults> GetAsync<T>(HttpGetCallResults statusCall, CancellationToken ct)
    {
        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(statusCall.StatusPath, ct);
            response.EnsureSuccessStatusCode();
            var StatusResults = await response.Content.ReadAsStringAsync(ct);
            try
            {
                // Use relaxed JSON options for deserialization
                statusCall.StatusResults = JsonSerializer.Deserialize<T>(StatusResults, _relaxedJsonOptions);
            }
            catch (Exception ex)
            {
                logger.LogCritical("HttpGetCallService:GetAsync:DeserializeException - {Message}", ex.Message);
                try
                {
                    // Fallback to dynamic deserialization with relaxed options
                    statusCall.StatusResults = JsonSerializer.Deserialize<dynamic>(StatusResults, _relaxedJsonOptions);
                }
                catch (Exception fallbackEx)
                {
                    logger.LogCritical("HttpGetCallService:GetAsync:FallbackDeserializeException - {Message}", fallbackEx.Message);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical("HttpGetCallService:GetAsync:Exception - {Message}", ex.Message);
        }
        return statusCall;
    }
}
