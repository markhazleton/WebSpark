using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace HttpClientUtility.RequestResult;

/// <summary>
/// The HttpRequestResultService class serves as the core service for sending HTTP requests.
/// </summary>
public class HttpRequestResultService(ILogger<HttpRequestResultService> logger, HttpClient httpClient) : IHttpRequestResultService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<HttpRequestResultService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Makes a request to the specified URL and returns the response.
    /// </summary>
    /// <typeparam name="T">The type of the expected response data.</typeparam>
    /// <param name="httpSendResults">A container for the URL to make the GET request to, and the expected response data.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A container for the response data and any relevant error information.</returns>
    public async Task<HttpRequestResult<T>> HttpSendRequestAsync<T>(HttpRequestResult<T> httpSendResults, CancellationToken ct)
    {
        ValidateHttpSendResults(httpSendResults);

        using var request = CreateHttpRequest(httpSendResults);
        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.SendAsync(request, ct).ConfigureAwait(true);

            // Check for redirects
            if (response?.StatusCode == HttpStatusCode.MovedPermanently)
            {
                httpSendResults.ErrorList.Add($"New: {response?.RequestMessage?.RequestUri} Old:{request?.RequestUri}");
                _logger.LogInformation("Redirected to {NewUrl}", response?.RequestMessage?.RequestUri);
            }

            return await ProcessHttpResponseAsync(response, httpSendResults, ct).ConfigureAwait(true);
        }
        catch (HttpRequestException ex)
        {
            httpSendResults.ErrorList.Add($"HttpRequestException: {ex.Message}");
            _logger.LogError(ex, "HttpSendRequestAsync:HttpRequestException {Message}", ex.Message);
            return httpSendResults;
        }
        catch (Exception ex)
        {
            httpSendResults.ErrorList.Add($"GeneralException: {ex.Message}");
            _logger.LogError(ex, "HttpSendRequestAsync:GeneralException {Message}", ex.Message);
            return httpSendResults;
        }
    }

    private void ValidateHttpSendResults(HttpRequestResultBase httpSendResults)
    {
        if (httpSendResults == null)
        {
            throw new ArgumentNullException(nameof(httpSendResults), "The parameter 'httpSendResults' cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(httpSendResults.RequestPath))
        {
            throw new ArgumentException("The URL path specified in 'httpSendResults' cannot be null or empty.", nameof(httpSendResults));
        }

        if (httpSendResults.RequestMethod == null)
        {
            throw new ArgumentException("The RequestMethod must be set.", nameof(httpSendResults));
        }
    }

    public HttpRequestMessage CreateHttpRequest(HttpRequestResultBase httpSendResults)
    {
        var request = new HttpRequestMessage(httpSendResults.RequestMethod, httpSendResults.RequestPath);

        if (httpSendResults.RequestHeaders != null)
        {
            foreach (var header in httpSendResults.RequestHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        if (request.Headers.UserAgent.Count == 0)
        {
            request.Headers.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
        }

        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");

        if (httpSendResults.RequestBody != null)
        {
            request.Content = httpSendResults.RequestBody;
        }

        return request;
    }

    private async Task<HttpRequestResult<T>> ProcessHttpResponseAsync<T>(HttpResponseMessage? response, HttpRequestResult<T> httpSendResults, CancellationToken ct)
    {
        if (response is null)
        {
            httpSendResults.StatusCode = HttpStatusCode.InternalServerError;
            return httpSendResults;
        }

        httpSendResults.StatusCode = response.StatusCode;
        string callResult = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (typeof(T) == typeof(string))
        {
            httpSendResults.ResponseResults = (T)(object)callResult;
        }
        else
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                    IgnoreReadOnlyFields = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    MaxDepth = 32,
                };

                httpSendResults.ResponseResults = JsonSerializer.Deserialize<T>(callResult);
            }
            catch (JsonException ex)
            {
                httpSendResults.ErrorList.Add($"HttpRequestResult:GetAsync:DeserializeException:{ex.Message}");
                _logger.LogCritical(ex, "HttpRequestResult:GetAsync:DeserializeException {Message}", ex.Message);
            }
        }

        return httpSendResults;
    }
}
