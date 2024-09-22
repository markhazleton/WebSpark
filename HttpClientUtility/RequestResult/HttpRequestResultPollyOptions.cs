namespace HttpClientUtility.RequestResult;

/// <summary>
/// Represents the options for HttpClientSendPolly.
/// </summary>
public class HttpRequestResultPollyOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; }

    /// <summary>
    /// Gets or sets the delay between retry attempts.
    /// </summary>
    public TimeSpan RetryDelay { get; set; }

    /// <summary>
    /// Gets or sets the threshold for circuit breaker.
    /// </summary>
    public int CircuitBreakerThreshold { get; set; }

    /// <summary>
    /// Gets or sets the duration of circuit breaker.
    /// </summary>
    public TimeSpan CircuitBreakerDuration { get; set; }
}
