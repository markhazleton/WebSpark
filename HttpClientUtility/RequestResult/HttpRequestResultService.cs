using HttpClientUtility.CurlService;
using HttpClientUtility.Utilities.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace HttpClientUtility.RequestResult;


/// <summary>
/// The HttpRequestResultService class serves as the core service for sending HTTP requests.
/// </summary>
/// <remarks>
/// This service implements standardized logging and error handling patterns
/// to ensure consistent behavior across all HTTP requests.
/// </remarks>
public class HttpRequestResultService(
    ILogger<HttpRequestResultService> _logger,
    IConfiguration _configuration,
    HttpClient httpClient) : IHttpRequestResultService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<HttpRequestResultService> _logger = _logger ?? throw new ArgumentNullException(nameof(_logger));
    private readonly CurlCommandSaver curlCommandSaver = new(_logger, _configuration);

    private async Task<HttpRequestResult<T>> ProcessHttpResponseAsync<T>(HttpResponseMessage? response, HttpRequestResult<T> httpSendResults, CancellationToken ct)
    {
        if (response is null)
        {
            httpSendResults.StatusCode = HttpStatusCode.InternalServerError;
            httpSendResults.AddError("Response was null");
            return httpSendResults;
        }

        httpSendResults.StatusCode = response.StatusCode;
        string callResult = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        // Log response details
        LoggingUtility.LogRequestCompletion(
            _logger,
            httpSendResults.RequestMethod.Method,
            httpSendResults.RequestPath,
            (int)response.StatusCode,
            httpSendResults.ElapsedMilliseconds,
            httpSendResults.CorrelationId);

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

                httpSendResults.ResponseResults = JsonSerializer.Deserialize<T>(callResult, options);

                if (httpSendResults.ResponseResults == null)
                {
                    httpSendResults.AddError("Failed to deserialize response to expected type");
                }
            }
            catch (JsonException ex)
            {
                // Create rich context for this exception
                var contextData = new Dictionary<string, object>
                {
                    ["ResponseStatusCode"] = response.StatusCode,
                    ["ResponseContentLength"] = callResult?.Length ?? 0,
                    ["ExpectedType"] = typeof(T).Name
                };

                // Log with rich context
                ErrorHandlingUtility.LogException(
                    ex,
                    _logger,
                    "JSON Deserialization",
                    httpSendResults.CorrelationId,
                    contextData);

                // Add to error list with context
                httpSendResults.ProcessException(ex, "Failed to deserialize response");
            }
        }

        return httpSendResults;
    }

    private static void ValidateHttpSendResults(HttpRequestResultBase httpSendResults)
    {
        if (httpSendResults == null)
        {
            throw new ArgumentNullException(nameof(httpSendResults), "The 'httpSendResults' parameter cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(httpSendResults.RequestPath))
        {
            throw new ArgumentException("The 'RequestPath' property in 'httpSendResults' cannot be null, empty, or whitespace.", nameof(httpSendResults.RequestPath));
        }
    }

    /// <summary>
    /// Creates an HTTP request message from the provided request result base.
    /// </summary>
    /// <param name="httpSendResults">The HTTP request result base containing request details</param>
    /// <returns>A configured HttpRequestMessage ready to be sent</returns>
    public HttpRequestMessage CreateHttpRequest(HttpRequestResultBase httpSendResults)
    {
        var request = new HttpRequestMessage(httpSendResults.RequestMethod, httpSendResults.RequestPath);

        // Add the correlation ID as a header for distributed tracing
        request.Headers.Add("X-Correlation-ID", httpSendResults.CorrelationId);

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

    /// <summary>
    /// Makes a request to the specified URL and returns the response.
    /// </summary>
    /// <typeparam name="T">The type of the expected response data.</typeparam>
    /// <param name="httpSendResults">A container for the URL to make the GET request to, and the expected response data.</param>
    /// <param name="memberName">Name of the calling member (automatically populated)</param>
    /// <param name="filePath">File path of the calling code (automatically populated)</param>
    /// <param name="lineNumber">Line number of the calling code (automatically populated)</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A container for the response data and any relevant error information.</returns>
    /// <remarks>
    /// This method implements a standardized error handling strategy:
    /// 1. Network errors (HttpRequestException) are logged as errors and returned as ServiceUnavailable
    /// 2. Timeouts (TaskCanceledException) are logged as warnings and returned as RequestTimeout
    /// 3. Server errors (5xx) are logged as errors and returned with original status code
    /// 4. Client errors (4xx) are logged as warnings and returned with original status code
    /// 5. Unexpected errors are logged as critical and returned as InternalServerError
    /// </remarks>
    public async Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(HttpRequestResult<T> httpSendResults,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
         CancellationToken ct = default)
    {
        // Start timing the operation
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Log the beginning of the request
            LoggingUtility.LogRequestStart(
                _logger,
                httpSendResults.RequestMethod.Method,
                httpSendResults.RequestPath,
                httpSendResults.CorrelationId);

            // Store caller context information
            httpSendResults.RequestContext["CallerMemberName"] = memberName;
            httpSendResults.RequestContext["CallerFilePath"] = filePath;
            httpSendResults.RequestContext["CallerLineNumber"] = lineNumber;

            // Step 1: Validate input data
            ValidateHttpSendResults(httpSendResults);

            // Step 2: Create the HTTP request with JSON content
            using var request = CreateHttpRequest(httpSendResults);

            // Ensure JSON content type
            if (request.Content != null && request.Content.Headers.ContentType?.MediaType != "application/json")
            {
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            }

            // Step 3: Build the curl command for debugging
            var curlCommand = new StringBuilder();
            curlCommand.Append("curl -X ");
            curlCommand.Append(request.Method.Method);
            curlCommand.Append(" '").Append(request.RequestUri).Append("'");

            // Add headers to the curl command
            foreach (var header in request.Headers)
            {
                // Skip correlation ID in the curl command for brevity
                if (header.Key.Equals("X-Correlation-ID", StringComparison.OrdinalIgnoreCase))
                    continue;

                curlCommand.Append(" -H '").Append(header.Key).Append(": ").Append(string.Join(",", header.Value)).Append("'");
            }

            // Add content type header for JSON in curl
            curlCommand.Append(" -H 'Content-Type: application/json'");

            // Add request body to the curl command if it's a POST, PUT, or PATCH request
            if (request.Content != null)
            {
                var content = await request.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                curlCommand.Append(" -d '").Append(content.Replace("'", "\\'")).Append("'");

                if (httpSendResults.IsDebugEnabled)
                {
                    _logger.LogDebug(
                        "Request Content [CorrelationId: {CorrelationId}]: {Content}",
                        httpSendResults.CorrelationId,
                        content);
                }
            }

            // Save the curl command for debugging
            await curlCommandSaver.SaveCurlCommandAsync(request, memberName, filePath, lineNumber).ConfigureAwait(false);

            // Step 4: Send the HTTP request
            _logger.LogDebug(
                "Sending HTTP {Method} request to {Url} [CorrelationId: {CorrelationId}]",
                request.Method.Method,
                LoggingUtility.SanitizeUrl(request.RequestUri.ToString()),
                httpSendResults.CorrelationId);

            HttpResponseMessage? response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);

            // Step 5: Handle response for redirects
            if (response?.StatusCode == HttpStatusCode.MovedPermanently)
            {
                httpSendResults.StatusCode = HttpStatusCode.MovedPermanently;
                httpSendResults.AddError($"Redirected from {request.RequestUri} to {response?.RequestMessage?.RequestUri}");

                _logger.LogWarning(
                    "Request redirected from {OriginalUrl} to {NewUrl} [CorrelationId: {CorrelationId}]",
                    LoggingUtility.SanitizeUrl(request.RequestUri.ToString()),
                    LoggingUtility.SanitizeUrl(response?.RequestMessage?.RequestUri?.ToString() ?? "unknown"),
                    httpSendResults.CorrelationId);
            }

            // Step 6: Process the response
            var result = await ProcessHttpResponseAsync(response, httpSendResults, ct).ConfigureAwait(false);

            // Record the timing information
            stopwatch.Stop();
            result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            result.CompletionDate = DateTime.UtcNow;

            return result;
        }
        catch (Exception ex)
        {
            // Stop the timer
            stopwatch.Stop();
            httpSendResults.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            httpSendResults.CompletionDate = DateTime.UtcNow;

            // Handle exception based on its type
            return await HandleExceptionAsync<T>(ex, httpSendResults, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles exceptions that occur during HTTP requests in a standardized way.
    /// </summary>
    /// <typeparam name="T">The type of the expected response.</typeparam>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="httpSendResults">The HTTP request results object.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An HTTP request result with appropriate error information.</returns>
    private async Task<HttpRequestResult<T>> HandleExceptionAsync<T>(
        Exception exception,
        HttpRequestResult<T> httpSendResults,
        CancellationToken ct)
    {
        // Get operation context for logging
        string operationName = $"HTTP {httpSendResults.RequestMethod.Method} {httpSendResults.SafeRequestPath}";

        // Process the exception to add context and record in error list
        Exception enrichedException = httpSendResults.ProcessException(exception, $"Error during {operationName}");

        // Set the appropriate status code based on the exception type
        httpSendResults.StatusCode = ErrorHandlingUtility.DetermineStatusCodeForException(exception);

        // Different handling based on exception type
        switch (exception)
        {
            case ArgumentNullException or ArgumentException:
                // Client-side validation error
                ErrorHandlingUtility.LogException(
                    enrichedException,
                    _logger,
                    operationName,
                    httpSendResults.CorrelationId);
                break;

            case HttpRequestException httpEx:
                // HTTP communication error
                _logger.Log(
                    httpEx.StatusCode.HasValue && (int)httpEx.StatusCode.Value < 500
                        ? LogLevel.Warning
                        : LogLevel.Error,
                    enrichedException,
                    "HTTP request failed with status {StatusCode}: {Message} [CorrelationId: {CorrelationId}]",
                    httpEx.StatusCode,
                    httpEx.Message,
                    httpSendResults.CorrelationId);
                break;

            case OperationCanceledException or TaskCanceledException:
                // Request was canceled
                _logger.LogWarning(
                    enrichedException,
                    "Request operation canceled [CorrelationId: {CorrelationId}]",
                    httpSendResults.CorrelationId);
                break;

            default:
                // Unexpected error
                _logger.LogError(
                    enrichedException,
                    "Unexpected error during {Operation}: {Message} [CorrelationId: {CorrelationId}]",
                    operationName,
                    enrichedException.Message,
                    httpSendResults.CorrelationId);
                break;
        }

        return httpSendResults;
    }
}
