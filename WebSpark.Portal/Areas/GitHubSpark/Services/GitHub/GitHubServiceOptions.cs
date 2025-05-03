namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Configuration options for GitHub services
/// </summary>
public class GitHubServiceOptions
{
    /// <summary>
    /// GitHub Personal Access Token for API authentication
    /// </summary>
    public string PersonalAccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Default user agent string used in API requests
    /// </summary>
    public string UserAgent { get; set; } = "WebSparkGitHubClient";

    /// <summary>
    /// Base cache duration in minutes for API responses
    /// </summary>
    public int DefaultCacheDurationMinutes { get; set; } = 300;

    /// <summary>
    /// Maximum API requests per hour (for custom rate limiting)
    /// </summary>
    public int MaxRequestsPerHour { get; set; } = 50;
}