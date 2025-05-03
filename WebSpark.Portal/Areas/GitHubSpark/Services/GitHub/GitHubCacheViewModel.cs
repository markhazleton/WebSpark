using HttpClientUtility.RequestResult;

namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// View model for GitHub user data including repositories, followers, and following
/// </summary>
public class GitHubCacheViewModel
{
    /// <summary>
    /// GitHub username
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Repository information for the user
    /// </summary>
    public HttpRequestResult<List<GitHubRepo>>? RepoInfo { get; set; }

    /// <summary>
    /// Basic user profile information
    /// </summary>
    public HttpRequestResult<GitHubUser>? User { get; set; }

    /// <summary>
    /// List of users following this user
    /// </summary>
    public HttpRequestResult<List<GitHubFollower>>? Followers { get; set; }

    /// <summary>
    /// List of users that this user follows
    /// </summary>
    public HttpRequestResult<List<GitHubFollower>>? Following { get; set; }

    /// <summary>
    /// Collection of errors that occurred during data retrieval
    /// </summary>
    public List<string> ErrorList { get; set; } = [];

    /// <summary>
    /// Timestamp when this data was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
