using HttpClientUtility.MemoryCache;
using HttpClientUtility.RequestResult;
using System.Net;

namespace WebSpark.Portal.Areas.AsyncSpark.Services.GitHub;



public class GitHubRepositoryService
{
    private const int MaxRepoLookupsPerHour = 50;
    private readonly IHttpRequestResultService _httpRequestResultService;
    private readonly IMemoryCacheManager _memoryCacheManager;
    private readonly string _token;
    private readonly string _repoLookupListCacheKey = "repo-lookup-list";

    public GitHubRepositoryService(IHttpRequestResultService httpClientSendService, IMemoryCacheManager memoryCacheManager, string token)
    {
        _httpRequestResultService = httpClientSendService;
        _memoryCacheManager = memoryCacheManager;
        _token = token;
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

    private async Task<T> GetGitHubRepoDataAsync<T>(string user, string repoName, string endpoint, CancellationToken ct)
    {
        var requestPath = string.IsNullOrEmpty(endpoint)
            ? $"https://api.github.com/repos/{user}/{repoName}"
            : $"https://api.github.com/repos/{user}/{repoName}/{endpoint}";

        var request = CreateGitHubRequest<T>(requestPath);
        var response = await _httpRequestResultService.HttpSendRequestResultAsync<T>(request, ct);

        if (response.StatusCode == HttpStatusCode.OK && response.ResponseResults != null)
        {
            return response.ResponseResults;
        }
        else
        {
            var errors = string.Join(", ", response.ErrorList);
            throw new Exception($"Failed to fetch data: {errors}");
        }
    }

    public async Task<GitHubRepositoryAnalysisViewModel> AnalyzeRepositoryAsync(string userName, string repoName, CancellationToken ct)
    {
        var repoTask = GetGitHubRepoDataAsync<GitHubRepo>(userName, repoName, string.Empty, ct);
        var contributorsTask = GetGitHubRepoDataAsync<List<GitHubContributor>>(userName, repoName, "contributors", ct);
        var languagesTask = GetGitHubRepoDataAsync<Dictionary<string, int>>(userName, repoName, "languages", ct);
        var issuesTask = GetGitHubRepoDataAsync<List<GitHubIssue>>(userName, repoName, "issues", ct);

        await Task.WhenAll(repoTask, contributorsTask, languagesTask, issuesTask);

        return new GitHubRepositoryAnalysisViewModel
        {
            Repository = await repoTask,
            Contributors = await contributorsTask,
            Languages = await languagesTask,
            Issues = await issuesTask,
            RepositoryName = repoName,
            UserName = userName
        };
    }

    public async Task<GitHubRepositoryAnalysisViewModel> GetRepositoryAnalysisAsync(string userName, string repoName, CancellationToken ct)
    {
        var repoList = _memoryCacheManager.Get<Dictionary<string, GitHubRepositoryAnalysisViewModel>>(_repoLookupListCacheKey, () => new Dictionary<string, GitHubRepositoryAnalysisViewModel>());

        var key = $"{userName}/{repoName}";

        if (repoList.TryGetValue(key, out var repoAnalysis)) return repoAnalysis;

        if (repoList.Count >= MaxRepoLookupsPerHour && !repoList.ContainsKey(key))
        {
            throw new Exception("API lookup limit exceeded.");
        }

        var fetchedData = await AnalyzeRepositoryAsync(userName, repoName, ct);

        repoList[key] = fetchedData;

        _memoryCacheManager.Set(_repoLookupListCacheKey, repoList, 60);
        return fetchedData;
    }
}
