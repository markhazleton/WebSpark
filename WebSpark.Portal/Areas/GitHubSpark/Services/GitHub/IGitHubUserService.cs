namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Interface for GitHub user service operations
/// </summary>
public interface IGitHubUserService
{
    /// <summary>
    /// Gets GitHub user data including repositories, followers, and following
    /// </summary>
    /// <param name="id">GitHub username</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>GitHub user data wrapped in cache view model</returns>
    Task<GitHubCacheViewModel> GetGitHubUserDataAsync(string id, CancellationToken ct);

    /// <summary>
    /// Fetches GitHub data directly from the API
    /// </summary>
    /// <param name="userName">GitHub username</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>GitHub user data wrapped in cache view model</returns>
    Task<GitHubCacheViewModel> FetchGitHubDataAsync(string userName, CancellationToken ct);

    /// <summary>
    /// Fetches GitHub data using the GraphQL API
    /// </summary>
    /// <param name="userName">GitHub username</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>GitHub user data wrapped in cache view model</returns>
    Task<GitHubCacheViewModel> FetchGitHubDataViaGraphQLAsync(string userName, CancellationToken ct);

    /// <summary>
    /// Searches for GitHub users using the GraphQL API
    /// </summary>
    /// <param name="searchQuery">Search query string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of users matching the search query</returns>
    Task<List<GitHubSearchUserResult>> SearchUsersViaGraphQLAsync(string searchQuery, CancellationToken ct);
}