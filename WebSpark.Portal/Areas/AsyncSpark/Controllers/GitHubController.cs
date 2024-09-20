using HttpClientUtility.SendService;
using WebSpark.Portal.Areas.AsyncSpark.Models.GitHub;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

public class GitHubController(
    IHttpClientSendService httpClientSendService,
    IConfiguration configuration) : AsyncSparkBaseController
{
    private readonly string _token = configuration["GitHubPAT"] ?? "MarkHazletonWebSpark";



    public async Task<IActionResult> User(string id, CancellationToken ct = default)
    {
        // Initiate all async calls in parallel
        var repoInfoTask = GetGitHubRepoAsync(id, ct);
        var userTask = GetGitHubUserAsync(id, ct);
        var followersTask = GetGitHubFollowersAsync(id, ct);
        var followingTask = GetGitHubFollowingAsync(id, ct);

        // Wait for all tasks to complete
        await Task.WhenAll(repoInfoTask,userTask, followersTask, followingTask);

        // Assign results to the view model
        GitHubCacheViewModel? cachedData = new()
        {
            RepoInfo = await repoInfoTask,
            User = await userTask,
            Followers = await followersTask,
            Following = await followingTask
        };

        return View("Index",cachedData);
    }
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        // Initiate all async calls in parallel
        var repoInfoTask = GetGitHubRepoAsync("markhazleton", ct);
        var userTask = GetGitHubUserAsync("markhazleton", ct);
        var followersTask = GetGitHubFollowersAsync("markhazleton", ct);
        var followingTask = GetGitHubFollowingAsync("markhazleton", ct);

        // Wait for all tasks to complete
        await Task.WhenAll(repoInfoTask, userTask, followersTask, followingTask);

        // Assign results to the view model
        GitHubCacheViewModel? cachedData = new()
        {
            RepoInfo = await repoInfoTask,
            User = await userTask,
            Followers = await followersTask,
            Following = await followingTask
        };

        return View(cachedData);
    }

    private async Task<HttpClientSendRequest<List<GitHubRepo>>> GetGitHubRepoAsync(string user, CancellationToken ct)
    {
        var gitHubRequest = new HttpClientSendRequest<List<GitHubRepo>>
        {
            CacheDurationMinutes = 60,
            RequestPath = $"https://api.github.com/users/{user}/repos"
        };
        gitHubRequest.RequestHeaders.Add("User-Agent", "MarkHazletonWebSpark");
        gitHubRequest.RequestHeaders.Add("Authorization", $"token {_token}");
        var repo = await httpClientSendService.HttpClientSendAsync<List<GitHubRepo>>(gitHubRequest, ct);
        return repo;
    }

    private async Task<HttpClientSendRequest<GitHubUser>> GetGitHubUserAsync(string user, CancellationToken ct)
    {
        var gitHubUserRequest = new HttpClientSendRequest<GitHubUser>
        {
            CacheDurationMinutes = 60,
            RequestPath = $"https://api.github.com/users/{user}"
        };
        gitHubUserRequest.RequestHeaders.Add("User-Agent", "MarkHazletonWebSpark");
        gitHubUserRequest.RequestHeaders.Add("Authorization", $"token {_token}");
        var userResponse = await httpClientSendService.HttpClientSendAsync<GitHubUser>(gitHubUserRequest, ct);
        return userResponse;
    }
    private async Task<HttpClientSendRequest<List<GitHubFollower>>> GetGitHubFollowersAsync(string user, CancellationToken ct)
    {
        var gitHubRequest = new HttpClientSendRequest<List<GitHubFollower>>
        {
            CacheDurationMinutes = 60,
            RequestPath = $"https://api.github.com/users/{user}/followers"
        };
        gitHubRequest.RequestHeaders.Add("User-Agent", "MarkHazletonWebSpark");
        gitHubRequest.RequestHeaders.Add("Authorization", $"token {_token}");
        var repo = await httpClientSendService.HttpClientSendAsync<List<GitHubFollower>>(gitHubRequest, ct);
        return repo;
    }
    private async Task<HttpClientSendRequest<List<GitHubFollower>>> GetGitHubFollowingAsync(string user, CancellationToken ct)
    {
        var gitHubRequest = new HttpClientSendRequest<List<GitHubFollower>>
        {
            CacheDurationMinutes = 60,
            RequestPath = $"https://api.github.com/users/{user}/following"
        };
        gitHubRequest.RequestHeaders.Add("User-Agent", "MarkHazletonWebSpark");
        gitHubRequest.RequestHeaders.Add("Authorization", $"token {_token}");
        var repo = await httpClientSendService.HttpClientSendAsync<List<GitHubFollower>>(gitHubRequest, ct);
        return repo;
    }


}
