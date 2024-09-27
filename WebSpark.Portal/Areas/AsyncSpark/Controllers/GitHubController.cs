using HttpClientUtility.MemoryCache;
using HttpClientUtility.RequestResult;
using WebSpark.Portal.Areas.AsyncSpark.Models.GitHub;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

public class GitHubController : AsyncSparkBaseController
{
    private readonly GitHubUserService _gitHubUserService;

    public GitHubController(
    IHttpRequestResultService httpClientSendService,
    IMemoryCacheManager _memoryCacheManager,
    IConfiguration configuration)
    {
        var _token = configuration["GitHubPAT"] ?? "MISSING";
        _gitHubUserService = new GitHubUserService(httpClientSendService, _memoryCacheManager, _token);
    }
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        return View("Index", await _gitHubUserService.GetGitHubUserDataAsync("markhazleton", ct));
    }
}

public class GitHubUserService
{
    private const int MaxLookupsPerHour = 20;
    private readonly IHttpRequestResultService _httpRequestResultService;
    private readonly IMemoryCacheManager _memoryCacheManager;
    private readonly string _token;
    private readonly string _userLookupListCacheKey = "user-lookup-list";

    public GitHubUserService(IHttpRequestResultService httpClientSendService, IMemoryCacheManager memoryCacheManager, string token)
    {
        _httpRequestResultService = httpClientSendService;
        _memoryCacheManager = memoryCacheManager;
        _token = token;
    }

    private void AddLookupLimitError(GitHubCacheViewModel cachedData)
    {
        cachedData.Followers.ErrorList.Add("You have reached the maximum number of lookups for this hour. Please try again later.");
        cachedData.Following.ErrorList.Add("You have reached the maximum number of lookups for this hour. Please try again later.");
    }


    private HttpRequestResult<T> CreateGitHubRequest<T>(string requestPath)
    {
        var request = new HttpRequestResult<T>
        {
            CacheDurationMinutes = 300,
            RequestPath = requestPath
        };
        request.RequestHeaders.Add("User-Agent", "MarkHazletonWebSpark");
        request.RequestHeaders.Add("Authorization", $"token {_token}");
        return request;
    }

    private async Task<HttpRequestResult<T>> GetGitHubDataAsync<T>(string user, string endpoint, CancellationToken ct)
    {
        var requestPath = string.IsNullOrEmpty(endpoint)
            ? $"https://api.github.com/users/{user}"
            : $"https://api.github.com/users/{user}/{endpoint}";

        var request = CreateGitHubRequest<T>(requestPath);
        return await _httpRequestResultService.HttpSendRequestAsync<T>(request, ct);
    }
    public async Task<GitHubCacheViewModel> FetchGitHubDataAsync(string userName, CancellationToken ct)
    {
        var userTask = GetGitHubDataAsync<GitHubUser>(userName, string.Empty, ct);
        var repoInfoTask = GetGitHubDataAsync<List<GitHubRepo>>(userName, "repos", ct);
        var followersTask = GetGitHubDataAsync<List<GitHubFollower>>(userName, "followers", ct);
        var followingTask = GetGitHubDataAsync<List<GitHubFollower>>(userName, "following", ct);

        await Task.WhenAll(repoInfoTask, userTask, followersTask, followingTask);

        return new GitHubCacheViewModel
        {
            RepoInfo = await repoInfoTask,
            User = await userTask,
            Followers = await followersTask,
            Following = await followingTask,
            UserName = userName
        };
    }

    public async Task<GitHubCacheViewModel> GetGitHubUserDataAsync(string id, CancellationToken ct)
    {
        var userList = _memoryCacheManager.Get<Dictionary<string, GitHubCacheViewModel>>(_userLookupListCacheKey, () => []);

        if (userList.TryGetValue(id, out var user)) return user;

        if (userList.Count >= MaxLookupsPerHour && !userList.ContainsKey(id))
        {
            id = "markhazleton";
            if (userList.TryGetValue(id, out var firstuser)) return firstuser;
        }
        var fetchedData = await FetchGitHubDataAsync(id, ct);

        userList.Add(id, fetchedData);

        if (userList.Count >= MaxLookupsPerHour)
        {
            AddLookupLimitError(fetchedData);
        }
        _memoryCacheManager.Set(_userLookupListCacheKey, userList, 60);
        return fetchedData;
    }
}
