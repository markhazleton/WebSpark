using HttpClientUtility.StringConverter;
using HttpClientUtility.Utilities.Logging;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace HttpClientUtility.ClientService;

/// <summary>
/// Represents a service for making HTTP requests using HttpClient.
/// </summary>
/// <remarks>
/// Initializes a new instance of the HttpClientService class with standardized logging and error handling.
/// </remarks>
public class HttpClientService : IHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IStringConverter _stringConverter;
    private readonly ILogger<HttpClientService> _logger;

    /// <summary>
    /// Initializes a new instance of the HttpClientService class.
    /// </summary>
    /// <param name="httpClientFactory">The factory for creating HttpClient instances.</param>
    /// <param name="stringConverter">The string converter for serializing and deserializing objects.</param>
    /// <param name="logger">The logger for capturing diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClientFactory or stringConverter is null.</exception>
    public HttpClientService(
        IHttpClientFactory httpClientFactory,
        IStringConverter stringConverter,
        ILogger<HttpClientService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _stringConverter = stringConverter ?? throw new ArgumentNullException(nameof(stringConverter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a configured HttpClient instance.
    /// </summary>
    /// <returns>The configured HttpClient instance.</returns>
    public HttpClient CreateConfiguredClient()
    {
        return _httpClientFactory.CreateClient("HttpClientService");
    }

    /// <summary>
    /// Executes an HTTP request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="T">The type of the response content.</typeparam>
    /// <param name="httpRequest">The function that sends the HTTP request.</param>
    /// <param name="method">The HTTP method being used.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="correlationId">The correlation ID for request tracking.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    private async Task<HttpResponseContent<T>> ExecuteRequestAsync<T>(
        Func<Task<HttpResponseMessage>> httpRequest,
        string method,
        Uri requestUri,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var sanitizedUri = LoggingUtility.SanitizeUrl(requestUri.ToString());

        try
        {
            // Log the beginning of the request
            LoggingUtility.LogRequestStart(_logger, method, requestUri.ToString(), correlationId);

            // Execute the request
            var response = await httpRequest().ConfigureAwait(false);

            // Stop timing
            stopwatch.Stop();

            // Log the completion of the request
            LoggingUtility.LogRequestCompletion(
                _logger,
                method,
                requestUri.ToString(),
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    var deserializedContent = _stringConverter.ConvertFromString<T>(content);
                    return HttpResponseContent<T>.Success(deserializedContent, response.StatusCode)
                        .WithCorrelationId(correlationId)
                        .WithElapsedTime(stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    // Handle deserialization error
                    var contextData = new Dictionary<string, object>
                    {
                        ["ResponseStatusCode"] = response.StatusCode,
                        ["ResponseContentLength"] = content?.Length ?? 0,
                        ["ExpectedType"] = typeof(T).Name,
                        ["RequestUri"] = sanitizedUri
                    };

                    ErrorHandlingUtility.LogException(
                        ex,
                        _logger,
                        "Deserialization",
                        correlationId,
                        contextData);

                    return HttpResponseContent<T>.Failure(
                        $"Deserialization Error: {ex.Message} (Ref: {correlationId})",
                        HttpStatusCode.UnprocessableEntity)
                        .WithCorrelationId(correlationId)
                        .WithElapsedTime(stopwatch.ElapsedMilliseconds);
                }
            }
            else
            {
                // Log the non-success status code
                var level = (int)response.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
                _logger.Log(
                    level,
                    "HTTP {Method} request to {RequestUri} failed with status {StatusCode}: {ReasonPhrase} [CorrelationId: {CorrelationId}]",
                    method,
                    sanitizedUri,
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    correlationId);

                return HttpResponseContent<T>.Failure(
                    $"Error: {response.ReasonPhrase} (Ref: {correlationId})",
                    response.StatusCode)
                    .WithCorrelationId(correlationId)
                    .WithElapsedTime(stopwatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            // Stop timing
            stopwatch.Stop();

            // Create rich context for the exception
            var contextData = new Dictionary<string, object>
            {
                ["Method"] = method,
                ["RequestUri"] = sanitizedUri,
                ["ElapsedMs"] = stopwatch.ElapsedMilliseconds
            };

            // Log the exception with enhanced context
            ErrorHandlingUtility.LogException(
                ex,
                _logger,
                $"HTTP {method}",
                correlationId,
                contextData);

            // Determine the appropriate status code
            var statusCode = ErrorHandlingUtility.DetermineStatusCodeForException(ex);

            // Create a response with the appropriate error information
            var errorMessage = $"{GetExceptionTypePrefix(ex)}: {ex.Message} (Ref: {correlationId})";

            return HttpResponseContent<T>.Failure(errorMessage, statusCode)
                .WithCorrelationId(correlationId)
                .WithElapsedTime(stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Gets a descriptive prefix for an exception based on its type.
    /// </summary>
    /// <param name="exception">The exception to analyze.</param>
    /// <returns>A descriptive prefix for the exception.</returns>
    private static string GetExceptionTypePrefix(Exception exception)
    {
        return exception switch
        {
            HttpRequestException => "HTTP Request Error",
            TaskCanceledException => "Request Timeout",
            OperationCanceledException => "Operation Canceled",
            JsonException => "JSON Parsing Error",
            _ => "Unexpected Error"
        };
    }

    private async Task<HttpResponseContent<TResult>> SendAsync<T, TResult>(
        HttpMethod method,
        Uri requestUri,
        T payload,
        Dictionary<string, string>? headers,
        CancellationToken cancellationToken)
    {
        // Generate a correlation ID for request tracking
        string correlationId = Guid.NewGuid().ToString();

        using var client = CreateConfiguredClient();

        try
        {
            var jsonPayload = _stringConverter.ConvertFromModel(payload);
            using var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(method, requestUri) { Content = httpContent };

            // Add correlation ID header for distributed tracing
            request.Headers.Add("X-Correlation-ID", correlationId);

            // Add custom headers if provided
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // Log detailed request info at debug level
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Sending HTTP {Method} request to {RequestUri} with payload size {PayloadSize} bytes [CorrelationId: {CorrelationId}]",
                    method.Method,
                    LoggingUtility.SanitizeUrl(requestUri.ToString()),
                    jsonPayload?.Length ?? 0,
                    correlationId);
            }

            return await ExecuteRequestAsync<TResult>(
                () => client.SendAsync(request, cancellationToken),
                method.Method,
                requestUri,
                correlationId,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // If an error occurs during request preparation, handle it with the same pattern
            var contextData = new Dictionary<string, object>
            {
                ["Method"] = method.Method,
                ["RequestUri"] = LoggingUtility.SanitizeUrl(requestUri.ToString())
            };

            ErrorHandlingUtility.LogException(
                ex,
                _logger,
                "Request Preparation",
                correlationId,
                contextData);

            var statusCode = ex switch
            {
                ArgumentException => HttpStatusCode.BadRequest,
                InvalidOperationException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };

            return HttpResponseContent<TResult>.Failure(
                $"Request Preparation Error: {ex.Message} (Ref: {correlationId})",
                statusCode)
                .WithCorrelationId(correlationId);
        }
    }

    /// <summary>
    /// Sends an HTTP DELETE request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="TResult">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    public async Task<HttpResponseContent<TResult>> DeleteAsync<TResult>(Uri requestUri, CancellationToken cancellationToken = default)
    {
        // Generate a correlation ID for request tracking
        string correlationId = Guid.NewGuid().ToString();

        using var client = CreateConfiguredClient();

        // Create a request message to add correlation ID header
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        request.Headers.Add("X-Correlation-ID", correlationId);

        return await ExecuteRequestAsync<TResult>(
            () => client.SendAsync(request, cancellationToken),
            HttpMethod.Delete.Method,
            requestUri,
            correlationId,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends an HTTP GET request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="T">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    public async Task<HttpResponseContent<T>> GetAsync<T>(Uri requestUri, CancellationToken cancellationToken = default)
    {
        // Generate a correlation ID for request tracking
        string correlationId = Guid.NewGuid().ToString();

        using var client = CreateConfiguredClient();

        // Create a request message to add correlation ID header
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("X-Correlation-ID", correlationId);

        return await ExecuteRequestAsync<T>(
            () => client.SendAsync(request, cancellationToken),
            HttpMethod.Get.Method,
            requestUri,
            correlationId,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends an HTTP POST request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="T">The type of the request payload.</typeparam>
    /// <typeparam name="TResult">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="payload">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    public Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default)
    {
        return SendAsync<T, TResult>(HttpMethod.Post, requestUri, payload, null, cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP POST request with headers asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="T">The type of the request payload.</typeparam>
    /// <typeparam name="TResult">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="payload">The request payload.</param>
    /// <param name="headers">The request headers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    public Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        return SendAsync<T, TResult>(HttpMethod.Post, requestUri, payload, headers, cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP PUT request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="T">The type of the request payload.</typeparam>
    /// <typeparam name="TResult">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="payload">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    public Task<HttpResponseContent<TResult>> PutAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default)
    {
        return SendAsync<T, TResult>(HttpMethod.Put, requestUri, payload, null, cancellationToken);
    }
}
