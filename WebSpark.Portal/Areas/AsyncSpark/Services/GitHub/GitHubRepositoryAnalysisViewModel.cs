using HttpClientUtility.MemoryCache;
using HttpClientUtility.RequestResult;

namespace WebSpark.Portal.Areas.AsyncSpark.Services.GitHub;


public class GitHubRepositoryAnalysisViewModel
{
    public GitHubRepo Repository { get; set; }
    public List<GitHubContributor> Contributors { get; set; }
    public Dictionary<string, int> Languages { get; set; }
    public List<GitHubIssue> Issues { get; set; }
    public string RepositoryName { get; set; }
    public string UserName { get; set; }

    // Helper properties to provide additional analysis or display-friendly data
    public int TotalContributors => Contributors?.Count ?? 0;
    public int TotalIssues => Issues?.Count ?? 0;
    public string PrimaryLanguage => Languages?.OrderByDescending(kv => kv.Value).FirstOrDefault().Key;

    // Error handling property for displaying API call issues
    public List<string> ErrorList { get; set; } = new List<string>();

    public GitHubRepositoryAnalysisViewModel()
    {
        Contributors = new List<GitHubContributor>();
        Languages = new Dictionary<string, int>();
        Issues = new List<GitHubIssue>();
    }
}
