namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Represents a GitHub user search result
/// </summary>
public class GitHubSearchUserResult
{
    /// <summary>
    /// The username/login of the user
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the user
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The URL to the user's avatar image
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// The URL to the user's GitHub profile
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The number of repositories owned by the user
    /// </summary>
    public int RepositoryCount { get; set; }

    /// <summary>
    /// The user's bio/description
    /// </summary>
    public string? Bio { get; set; }
}