using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace HttpClientUtility.RequestResult;


/// <summary>
/// Represents a HttpRequestResultService implementation that uses Polly for retry and circuit breaker policies.
/// </summary>
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

        // Configure the retry policy
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(options.MaxRetryAttempts, retryAttempt => options.RetryDelay,
                (exception, timespan, retryCount, context) =>
                {
                    // Optionally, you can log or handle the retry attempt here
                    _errorList.Add($"Polly.RetryPolicy Retries:{retryCount}, exception:{exception.Message}");
                });

        // Configure the circuit breaker policy
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(options.CircuitBreakerThreshold, options.CircuitBreakerDuration,
                (exception, duration) =>
                {
                    // Optionally, you can log or handle the circuit breaker state change here
                    _errorList.Add($"Polly.CircuitBreaker: duration{duration.TotalSeconds} exception:{exception.Message}");
                },
                () =>
                {
                    // Optionally, you can handle the circuit breaker being reset here
                    _errorList.Add($"Polly.CircuitBreaker: RESET");
                });
    }

    /// <summary>
    /// Sends an HTTP request asynchronously using the HttpRequestResultServicePolly implementation.
    /// </summary>
    /// <typeparam name="T">The type of the response content.</typeparam>
    /// <param name="statusCall">The HttpRequestResult object representing the request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The HttpRequestResult object with the response content.</returns>
    public async Task<HttpRequestResult<T>> HttpSendRequestAsync<T>(HttpRequestResult<T> statusCall, CancellationToken ct)
    {
        // Wrap the GetAsync call with the circuit breaker policies
        try
        {
            statusCall = await _circuitBreakerPolicy.ExecuteAsync(() => _service.HttpSendRequestAsync(statusCall, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Polly:CircuitBreaker:Exception:{ex.Message}");
            _errorList.Add($"Polly:GetAsync:Exception:{ex.Message}");
        }

        statusCall.ErrorList.AddRange(_errorList);

        _errorList.Clear();

        return statusCall;
    }
}
