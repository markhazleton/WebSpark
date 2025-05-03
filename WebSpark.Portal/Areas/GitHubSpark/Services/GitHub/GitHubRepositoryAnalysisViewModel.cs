namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// View model for GitHub repository analysis results including code structure, contributors, and issues
/// </summary>
public class GitHubRepositoryAnalysisViewModel
{
    /// <summary>
    /// Basic repository information
    /// </summary>
    public GitHubRepo Repository { get; set; }

    /// <summary>
    /// List of contributors to the repository
    /// </summary>
    public List<GitHubContributor> Contributors { get; set; }

    /// <summary>
    /// Breakdown of programming languages used in the repository
    /// </summary>
    public Dictionary<string, int> Languages { get; set; }

    /// <summary>
    /// List of open issues in the repository
    /// </summary>
    public List<GitHubIssue> Issues { get; set; }

    /// <summary>
    /// File system tree representation of the repository
    /// </summary>
    public FileSystemNode FileSystemTree { get; set; }

    /// <summary>
    /// Name of the repository
    /// </summary>
    public string RepositoryName { get; set; }

    /// <summary>
    /// Username of the repository owner
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Timestamp when this data was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the total number of contributors to the repository
    /// </summary>
    public int TotalContributors => Contributors?.Count ?? 0;

    /// <summary>
    /// Gets the total number of open issues in the repository
    /// </summary>
    public int TotalIssues => Issues?.Count ?? 0;

    /// <summary>
    /// Gets the primary programming language used in the repository
    /// </summary>
    public string PrimaryLanguage => Languages?.OrderByDescending(kv => kv.Value).FirstOrDefault().Key;

    /// <summary>
    /// Collection of errors that occurred during data retrieval
    /// </summary>
    public List<string> ErrorList { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the GitHubRepositoryAnalysisViewModel class
    /// </summary>
    public GitHubRepositoryAnalysisViewModel()
    {
        Contributors = [];
        Languages = [];
        Issues = [];
    }
}

