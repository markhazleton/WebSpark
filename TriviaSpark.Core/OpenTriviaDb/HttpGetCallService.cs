using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TriviaSpark.Core.OpenTriviaDb;

public class HttpGetCallService : IHttpGetCallService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpGetCallService> _logger;

    public HttpGetCallService(ILogger<HttpGetCallService> logger, IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("TriviaSpark");
        _logger = logger;
    }
    /// <summary>
    /// Makes a GET request to the specified URL and returns the response.
    /// </summary>
    /// <typeparam name="T">The type of the expected response data.</typeparam>
    /// <param name="getCallResults">A container for the URL to make the GET request to, and the expected response data.</param>
    /// <returns>A container for the response data and any relevant error information.</returns>
    /// <param name="ct"></param>
    public async Task<HttpGetCallResults<T>> GetAsync<T>(HttpGetCallResults<T> getCallResults, CancellationToken ct)
    {
        int retryCount = 0;
        int maxRetries = 3;

        if (getCallResults == null)
        {
            _logger.LogCritical("The parameter 'getCallResults' cannot be null.");
            throw new ArgumentNullException(nameof(getCallResults), "The parameter 'getCallResults' cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(getCallResults.RequestPath))
        {
            _logger.LogCritical("The URL path specified in 'getCallResults' cannot be null or empty.");
            throw new ArgumentException("The URL path specified in 'getCallResults' cannot be null or empty.", nameof(getCallResults));
        }

        while (retryCount < maxRetries + 1)
        {
            try
            {
                getCallResults.Retries = retryCount;
                using var request = new HttpRequestMessage(HttpMethod.Get, getCallResults.RequestPath);
                request.Version = new Version(2, 0);
                request.Headers.ConnectionClose = false;
                using HttpResponseMessage response = await _httpClient.SendAsync(request, ct);
                response.EnsureSuccessStatusCode();
                string callResult = await response.Content.ReadAsStringAsync(ct);
                try
                {
                    getCallResults.ResponseResults = JsonSerializer.Deserialize<T>(callResult);
                    return getCallResults;
                }
                catch (Exception ex)
                {
                    getCallResults.ErrorMessage = $"HttpGetCallService:GetAsync:DeserializeException:{ex.Message}";
                    _logger.LogCritical("HttpGetCallService:GetAsync:DeserializeException", ex.Message);
                    return getCallResults;
                }
            }
            catch (Exception ex)
            {
                getCallResults.ErrorMessage = $"HttpGetCallService:GetAsync:Exception:{ex.Message}";
                if (++retryCount >= maxRetries)
                {
                    _logger.LogCritical("HttpGetCallService:GetAsync:Exception", new { retryCount, ex.Message });
                    return getCallResults;
                }
            }
        }
        return getCallResults;
    }
}



