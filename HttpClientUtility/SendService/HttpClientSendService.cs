using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HttpClientUtility.SendService;

/// <summary>
/// The HttpClientSendService class serves as the core service for sending HTTP requests.
/// It implements the IHttpClientSendService interface and provides methods for sending HTTP requests,
/// validating input parameters, and processing HTTP responses.
/// </summary>
/// <remarks>
/// This class is designed to be robust and extensible, with features like:
/// - Asynchronous HTTP request sending using HttpClient.
/// - Exception handling for various types of exceptions like HttpRequestException.
/// - Logging capabilities for capturing both informational messages and errors.
/// - JSON serialization and deserialization for handling response data.
/// </remarks>
/// <example>
/// Here is a simple example of using HttpClientSendService:
/// <code>
/// var httpClientSendService = new HttpClientSendService(logger, httpClient);
/// var response = await httpClientSendService.HttpClientSendAsync(httpSendResults, CancellationToken.None);
/// </code>
/// </example>
public class HttpClientSendService(ILogger<HttpClientSendService> logger, HttpClient httpClient) : IHttpClientSendService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<HttpClientSendService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Version _httpVersion = new(2, 0);
    private const bool ConnectionClose = false;

    /// <summary>
    /// Makes a request to the specified URL and returns the response.
    /// </summary>
    /// <typeparam name="T">The type of the expected response data.</typeparam>
    /// <param name="httpSendResults">A container for the URL to make the GET request to, and the expected response data.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A container for the response data and any relevant error information.</returns>
    public async Task<HttpClientSendRequest<T>> HttpClientSendAsync<T>(HttpClientSendRequest<T> httpSendResults, CancellationToken ct)
    {
        ValidateHttpSendResults(httpSendResults);

        using var request = CreateHttpRequest(httpSendResults);
        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.SendAsync(request, ct).ConfigureAwait(true);
            // Check for redirects
            if (response?.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
            {
                httpSendResults.ErrorList.Add($"New: {response?.RequestMessage?.RequestUri} Old:{request?.RequestUri}");
                _logger.LogInformation("Redirected to {NewUrl}", response?.RequestMessage?.RequestUri);
            }
            return await ProcessHttpResponseAsync(response, httpSendResults, ct).ConfigureAwait(true);
        }
        catch (HttpRequestException ex)
        {
            httpSendResults.ErrorList.Add($"HttpRequestException: {ex.Message}");
            _logger.LogError(ex, "HttpClientSendAsync:HttpRequestException {Message}", ex.Message);
            return httpSendResults; // Early return if the request failed
        }
        catch (Exception ex) // Catch other types of exceptions here
        {
            httpSendResults.ErrorList.Add($"GeneralException: {ex.Message}");
            _logger.LogError(ex, "HttpClientSendAsync:GeneralException {Message}", ex.Message);
            return httpSendResults;
        }
    }

    private void ValidateHttpSendResults<T>(HttpClientSendRequest<T> httpSendResults)
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
    /// <summary>
    /// Create HttpRequestMessage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="httpSendResults"></param>
    /// <returns></returns>
    public HttpRequestMessage CreateHttpRequest<T>(HttpClientSendRequest<T> httpSendResults)
    {
        var request = new HttpRequestMessage(httpSendResults.RequestMethod, httpSendResults.RequestPath)
        {
            Version = _httpVersion,
            Headers = { ConnectionClose = ConnectionClose }
        };

        if (httpSendResults.RequestBody != null)
        {
            request.Content = httpSendResults.RequestBody;
        }

        return request;
    }

    private async Task<HttpClientSendRequest<T>> ProcessHttpResponseAsync<T>(HttpResponseMessage? response, HttpClientSendRequest<T> httpSendResults, CancellationToken ct)
    {
        if (response is null)
        {
            httpSendResults.StatusCode = System.Net.HttpStatusCode.InternalServerError;
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
                httpSendResults.ResponseResults = JsonSerializer.Deserialize<T>(callResult);
            }
            catch (JsonException ex)
            {
                httpSendResults.ErrorList.Add($"HttpClientSendRequest:GetAsync:DeserializeException:{ex.Message}");
                _logger.LogCritical(ex, "HttpClientSendRequest:GetAsync:DeserializeException {Message}", ex.Message);
            }
        }

        return httpSendResults;
    }
}
