using HttpClientUtility.SendService;
using WebSpark.Portal.Areas.AsyncSpark.Models.GitHub;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

public class GitHubController(
    IHttpClientFactory httpClientFactory,
    IHttpClientSendService httpClientSendService,
    IConfiguration configuration) : AsyncSparkBaseController
{
    private readonly string _token = configuration["GitHubPAT"] ?? "MarkHazletonWebSpark";

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        GitHubUser user = await GetGitHubUserAsync("markhazleton",ct);
        GitHubRepo repo = await GetGitHubRepoAsync("markhazleton", "SampleMvcCRUD", ct);
        GitHubCacheViewModel? cachedData = new()
        {
            RepoInfo = repo,
            User = user
        };
        return View(cachedData);
    }
    private async Task<GitHubRepo> GetGitHubRepoAsync(string user, string repoName, CancellationToken ct)
    {
        var gitHubRequest = new HttpClientSendRequest<GitHubRepo>
        {
            CacheDurationMinutes = 60,
            RequestPath = $"https://api.github.com/repos/{user}/{repoName}"
        };
        gitHubRequest.RequestHeaders.Add("User-Agent", "MarkHazletonWebSpark");
        gitHubRequest.RequestHeaders.Add("Authorization", $"token {_token}");
        var repo = await httpClientSendService.HttpClientSendAsync<GitHubRepo>(gitHubRequest, ct);
        return repo.ResponseResults;
    }

    private async Task<GitHubUser> GetGitHubUserAsync(string user,CancellationToken ct)
    {
        var gitHubUserRequest = new HttpClientSendRequest<GitHubUser>
        {
            CacheDurationMinutes = 60,
            RequestPath = $"https://api.github.com/users/{user}"
        };
        gitHubUserRequest.RequestHeaders.Add("User-Agent", "MarkHazletonWebSpark");
        gitHubUserRequest.RequestHeaders.Add("Authorization", $"token {_token}");
        var userResponse = await httpClientSendService.HttpClientSendAsync<GitHubUser>(gitHubUserRequest, ct);
        return userResponse.ResponseResults;
    }
}
