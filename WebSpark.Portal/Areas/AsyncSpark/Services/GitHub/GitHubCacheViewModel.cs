using HttpClientUtility.RequestResult;

namespace WebSpark.Portal.Areas.AsyncSpark.Services.GitHub;

public class GitHubCacheViewModel
{
    public string UserName { get; set; }
    public HttpRequestResult<List<GitHubRepo>>? RepoInfo { get; set; }
    public HttpRequestResult<GitHubUser>? User { get; set; }
    public HttpRequestResult<List<GitHubFollower>>? Followers { get; set; }
    public HttpRequestResult<List<GitHubFollower>>? Following { get; set; }
    public List<string> ErrorList { get; set; } = [];
}
