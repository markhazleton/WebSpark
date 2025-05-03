namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Contains information about GitHub API rate limits
/// </summary>
public class RateLimitInfo
{
    /// <summary>
    /// The maximum number of requests allowed in the current time period
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// The number of requests remaining in the current time period
    /// </summary>
    public int Remaining { get; set; }

    /// <summary>
    /// The time when the current rate limit will reset
    /// </summary>
    public DateTime? Reset { get; set; }

    /// <summary>
    /// Returns a string representation of the rate limit information
    /// </summary>
    public override string ToString()
    {
        return $"Limit: {Limit}, Remaining: {Remaining}, Reset: {(Reset.HasValue ? Reset.Value.ToString("yyyy-MM-dd HH:mm:ss UTC") : "unknown")}";
    }
}