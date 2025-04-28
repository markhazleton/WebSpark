using HttpClientUtility.Utilities.Logging;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HttpClientUtility.RequestResult;

/// <summary>
/// Represents a HttpRequestResultService implementation that uses Polly for retry and circuit breaker policies.
/// </summary>
/// <remarks>
/// This service adds resilience patterns (retry and circuit breaker) with standardized logging
/// and error handling to HTTP requests.
/// </remarks>
public class HttpRequestResultServicePolly : IHttpRequestResultService
{
    private readonly ILogger<HttpRequestResultServicePolly> _logger;
    private readonly List<string> _errorList = [];
    private readonly IHttpRequestResultService _service;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly HttpRequestResultPollyOptions _options;

    /// <summary>
    /// Initializes a new instance of the HttpRequestResultServicePolly class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="service">The underlying HttpClientService.</param>
    /// <param name="options">The HttpRequestResultPollyOptions.</param>
    public HttpRequestResultServicePolly(
        ILogger<HttpRequestResultServicePolly>? logger,
        IHttpRequestResultService? service,
        HttpRequestResultPollyOptions? options)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));

        // Configure the retry policy with enhanced logging
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                options.MaxRetryAttempts,
                retryAttempt => options.RetryDelay,
                (exception, timespan, retryCount, context) =>
                {
                    string correlationId = context.ContainsKey("correlationId")
                        ? context["correlationId"]?.ToString() ?? Guid.NewGuid().ToString()
                        : Guid.NewGuid().ToString();

                    // Create rich logging context
                    var contextData = new Dictionary<string, object>
                    {
                        ["RetryCount"] = retryCount,
                        ["RetryDelay"] = timespan.TotalMilliseconds,
                        ["MaxRetries"] = options.MaxRetryAttempts
                    };

                    // Log the retry with detailed information
                    _logger.LogWarning(
                        exception,
                        "Retry {RetryCount}/{MaxRetries} after {RetryDelay}ms due to: {ErrorMessage} [CorrelationId: {CorrelationId}]",
                        retryCount,
                        options.MaxRetryAttempts,
                        timespan.TotalMilliseconds,
                        exception.Message,
                        correlationId);

                    _errorList.Add($"Retry {retryCount}/{options.MaxRetryAttempts}: {exception.Message} [CorrelationId: {correlationId}]");
                });

        // Configure the circuit breaker policy with enhanced logging
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                options.CircuitBreakerThreshold,
                options.CircuitBreakerDuration,
                OnCircuitBreak,
                OnCircuitReset);
    }

    /// <summary>
    /// Handles circuit breaking events with standardized logging.
    /// </summary>
    private void OnCircuitBreak(Exception exception, TimeSpan duration)
    {
        var correlationId = Guid.NewGuid().ToString();

        // Create rich logging context
        var contextData = new Dictionary<string, object>
        {
            ["CircuitBreakDuration"] = duration.TotalSeconds,
            ["Threshold"] = _options.CircuitBreakerThreshold,
            ["ExceptionType"] = exception.GetType().Name
        };

        // Log with rich context using our standardized approach
        ErrorHandlingUtility.LogException(
            exception,
            _logger,
            "Circuit Breaker Opened",
            correlationId,
            contextData);

        _logger.LogError(
            "Circuit breaker opened for {DurationSeconds}s due to: {ErrorMessage} [CorrelationId: {CorrelationId}]",
            duration.TotalSeconds,
            exception.Message,
            correlationId);

        _errorList.Add($"Circuit breaker opened for {duration.TotalSeconds}s: {exception.Message} [CorrelationId: {correlationId}]");
    }

    /// <summary>
    /// Handles circuit reset events with standardized logging.
    /// </summary>
    private void OnCircuitReset()
    {
        var correlationId = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Circuit breaker reset - service recovered [CorrelationId: {CorrelationId}]",
            correlationId);

        _errorList.Add($"Circuit breaker reset [CorrelationId: {correlationId}]");
    }

    /// <summary>
    /// Sends an HTTP request asynchronously using the HttpRequestResultServicePolly implementation.
    /// </summary>
    /// <typeparam name="T">The type of the response content.</typeparam>
    /// <param name="statusCall">The HttpRequestResult object representing the request.</param>
    /// <param name="memberName">Name of the calling member (automatically populated)</param>
    /// <param name="filePath">File path of the calling code (automatically populated)</param>
    /// <param name="lineNumber">Line number of the calling code (automatically populated)</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The HttpRequestResult object with the response content.</returns>
    /// <remarks>
    /// This method implements resilience patterns with standardized error handling:
    /// 1. Circuit breaker pattern prevents cascading failures when a service is unavailable
    /// 2. Retry pattern handles transient failures with exponential backoff
    /// 3. All exceptions and policy events are logged with correlation IDs for traceability
    /// </remarks>
    public async Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> statusCall,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        string correlationId = statusCall.CorrelationId;
        string requestPath = statusCall.SafeRequestPath;

        // Create context data to share with policies
        var pollyContext = new Context
        {
            ["correlationId"] = correlationId,
            ["requestPath"] = requestPath,
            ["requestMethod"] = statusCall.RequestMethod.Method
        };

        // Log the beginning of the resilient request
        _logger.LogInformation(
            "Beginning resilient HTTP {Method} request to {Path} with circuit state: {CircuitState} [CorrelationId: {CorrelationId}]",
            statusCall.RequestMethod.Method,
            requestPath,
            _circuitBreakerPolicy.CircuitState,
            correlationId);

        statusCall.RequestContext["CircuitState"] = _circuitBreakerPolicy.CircuitState.ToString();
        statusCall.RequestContext["MaxRetries"] = _options.MaxRetryAttempts;

        try
        {
            // Execute the request with the circuit breaker and retry policies
            statusCall = await _circuitBreakerPolicy
                .ExecuteAsync(ctx => _service.HttpSendRequestResultAsync(
                    statusCall,
                    memberName,
                    filePath,
                    lineNumber,
                    ct),
                pollyContext)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Create rich context data for logging
            var contextData = new Dictionary<string, object>
            {
                ["CircuitState"] = _circuitBreakerPolicy.CircuitState.ToString(),
                ["RequestPath"] = requestPath,
                ["RequestMethod"] = statusCall.RequestMethod.Method
            };

            // Log the exception with rich context
            ErrorHandlingUtility.LogException(
                ex,
                _logger,
                "Resilience Policy",
                correlationId,
                contextData);

            statusCall.ProcessException(ex, "Resilience policy error");
        }
        finally
        {
            // Stop timing and record metrics
            stopwatch.Stop();

            // Add all error messages from retries and circuit breaker events
            statusCall.ErrorList.AddRange(_errorList);
            _errorList.Clear();

            // Add context information
            statusCall.RequestContext["ResilienceLayerDurationMs"] = stopwatch.ElapsedMilliseconds;
            statusCall.RequestContext["FinalCircuitState"] = _circuitBreakerPolicy.CircuitState.ToString();

            // Log completion
            _logger.LogInformation(
                "Completed resilient HTTP {Method} request to {Path} in {DurationMs}ms with status {Status} [CorrelationId: {CorrelationId}]",
                statusCall.RequestMethod.Method,
                requestPath,
                stopwatch.ElapsedMilliseconds,
                statusCall.StatusCode,
                correlationId);
        }

        return statusCall;
    }
}
