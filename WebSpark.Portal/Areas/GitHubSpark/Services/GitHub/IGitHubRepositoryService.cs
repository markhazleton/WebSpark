namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Interface for GitHub repository services
/// </summary>
public interface IGitHubRepositoryService
{
    /// <summary>
    /// Gets GitHub repository analysis data including contributors, languages, and issues
    /// </summary>
    /// <param name="userName">GitHub username</param>
    /// <param name="repoName">Repository name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Repository analysis data</returns>
    Task<GitHubRepositoryAnalysisViewModel> GetRepositoryAnalysisAsync(string userName, string repoName, CancellationToken ct);

    /// <summary>
    /// Analyzes a GitHub repository with detailed file and code structure
    /// </summary>
    /// <param name="userName">GitHub username</param>
    /// <param name="repoName">Repository name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Detailed repository analysis</returns>
    Task<GitHubRepositoryAnalysisViewModel> AnalyzeRepositoryAsync(string userName, string repoName, CancellationToken ct);
}