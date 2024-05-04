namespace HttpClientUtility.SendService;

public class HttpClientSendPollyOptions
{
    public int MaxRetryAttempts { get; set; }
    public TimeSpan RetryDelay { get; set; }
    public int CircuitBreakerThreshold { get; set; }
    public TimeSpan CircuitBreakerDuration { get; set; }
}
