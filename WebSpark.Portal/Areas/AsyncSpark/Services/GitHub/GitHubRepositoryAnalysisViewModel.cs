namespace WebSpark.Portal.Areas.AsyncSpark.Services.GitHub;

public class GitHubRepositoryAnalysisViewModel
{
    public GitHubRepo Repository { get; set; }
    public List<GitHubContributor> Contributors { get; set; }
    public Dictionary<string, int> Languages { get; set; }
    public List<GitHubIssue> Issues { get; set; }
    public FileSystemNode FileSystemTree { get; set; }
    public string RepositoryName { get; set; }
    public string UserName { get; set; }

    public int TotalContributors => Contributors?.Count ?? 0;
    public int TotalIssues => Issues?.Count ?? 0;
    public string PrimaryLanguage => Languages?.OrderByDescending(kv => kv.Value).FirstOrDefault().Key;

    public List<string> ErrorList { get; set; } = [];

    public GitHubRepositoryAnalysisViewModel()
    {
        Contributors = [];
        Languages = [];
        Issues = [];
    }
}

