using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Net;
using WebSpark.HttpClientUtility.MemoryCache;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Service for interacting with GitHub user-related APIs
/// </summary>
public class GitHubUserService : IGitHubUserService
{
    private readonly IHttpRequestResultService _httpRequestResultService;
    private readonly IMemoryCacheManager _memoryCacheManager;
    private readonly ILogger<GitHubUserService> _logger;
    private readonly GitHubServiceOptions _options;
    private readonly string _userLookupListCacheKey = "user-lookup-list";
    private readonly AsyncRetryPolicy<HttpRequestResult<object>> _retryPolicy;

    /// <summary>
    /// Creates a new instance of the GitHub user service
    /// </summary>
    public GitHubUserService(
        IHttpRequestResultService httpClientSendService,
        IMemoryCacheManager memoryCacheManager,
        ILogger<GitHubUserService> logger,
        IOptions<GitHubServiceOptions> options)
    {
        _httpRequestResultService = httpClientSendService;
        _memoryCacheManager = memoryCacheManager;
        _logger = logger;
        _options = options.Value;

        // Create a retry policy for transient HTTP errors
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .OrResult<HttpRequestResult<object>>(r =>
                r.StatusCode == HttpStatusCode.TooManyRequests ||
                r.StatusCode == HttpStatusCode.RequestTimeout ||
                (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                3, // Retry count
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning("Retrying GitHub API request (attempt {RetryAttempt}) after {RetryTimespan} due to: {ErrorMessage}",
                        retryAttempt, timespan, outcome.Exception?.Message ?? "HTTP error");
                });
    }

    private void AddLookupLimitError(GitHubCacheViewModel cachedData)
    {
        string errorMessage = "You have reached the maximum number of lookups for this hour. Please try again later.";
        cachedData.Followers.ErrorList.Add(errorMessage);
        cachedData.Following.ErrorList.Add(errorMessage);
        _logger.LogWarning("Rate limit exceeded: {ErrorMessage}", errorMessage);
    }

    private HttpRequestResult<T> CreateGitHubRequest<T>(string requestPath)
    {
        var request = new HttpRequestResult<T>
        {
            CacheDurationMinutes = _options.DefaultCacheDurationMinutes,
            RequestPath = requestPath
        };
        request.RequestHeaders.Add("User-Agent", _options.UserAgent);
        request.RequestHeaders.Add("Authorization", $"token {_options.PersonalAccessToken}");

        // Add accept header for GitHub API v3
        request.RequestHeaders.Add("Accept", "application/vnd.github.v3+json");

        return request;
    }

    private async Task<HttpRequestResult<T>> GetGitHubDataAsync<T>(string user, string endpoint, CancellationToken ct)
    {
        var requestPath = string.IsNullOrEmpty(endpoint)
            ? $"https://api.github.com/users/{user}"
            : $"https://api.github.com/users/{user}/{endpoint}";

        var request = CreateGitHubRequest<T>(requestPath);

        try
        {
            _logger.LogDebug("Sending GitHub API request to {RequestPath}", requestPath);
            return await _httpRequestResultService.HttpSendRequestResultAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub data from {RequestPath}", requestPath);
            throw new GitHubApiException($"Failed to fetch data from GitHub API: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<GitHubCacheViewModel> FetchGitHubDataAsync(string userName, CancellationToken ct)
    {
        _logger.LogInformation("Fetching GitHub data for user {UserName}", userName);

        try
        {
            var userTask = GetGitHubDataAsync<GitHubUser>(userName, string.Empty, ct);
            var repoInfoTask = GetGitHubDataAsync<List<GitHubRepo>>(userName, "repos", ct);
            var followersTask = GetGitHubDataAsync<List<GitHubFollower>>(userName, "followers", ct);
            var followingTask = GetGitHubDataAsync<List<GitHubFollower>>(userName, "following", ct);

            await Task.WhenAll(userTask, repoInfoTask, followersTask, followingTask);

            var result = new GitHubCacheViewModel
            {
                RepoInfo = await repoInfoTask,
                User = await userTask,
                Followers = await followersTask,
                Following = await followingTask,
                UserName = userName,
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully fetched GitHub data for {UserName}", userName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FetchGitHubDataAsync for user {UserName}", userName);
            throw new GitHubApiException($"Failed to fetch GitHub data for user {userName}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<GitHubCacheViewModel> GetGitHubUserDataAsync(string id, CancellationToken ct)
    {
        try
        {
            // Check cache first
            var userList = _memoryCacheManager.Get(
                _userLookupListCacheKey,
                () => new Dictionary<string, GitHubCacheViewModel>());

            // Return cached data if available
            if (userList.TryGetValue(id, out var user))
            {
                _logger.LogDebug("Cache hit for GitHub user {UserId}", id);
                return user;
            }

            // Check rate limiting
            if (userList.Count >= _options.MaxRequestsPerHour && !userList.ContainsKey(id))
            {
                _logger.LogWarning("Rate limit reached, returning default user data for {DefaultUser}", "markhazleton");
                id = "markhazleton";
                if (userList.TryGetValue(id, out var defaultUser))
                    return defaultUser;
            }

            _logger.LogInformation("Cache miss for GitHub user {UserId}, fetching from API", id);
            var fetchedData = await FetchGitHubDataAsync(id, ct);

            // Add to cache
            userList[id] = fetchedData;

            // Apply rate limit errors if needed
            if (userList.Count >= _options.MaxRequestsPerHour)
            {
                AddLookupLimitError(fetchedData);
            }

            // Update cache with longer duration
            _memoryCacheManager.Set(_userLookupListCacheKey, userList, 600);
            return fetchedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetGitHubUserDataAsync for user {UserId}", id);
            throw new GitHubApiException($"Failed to get GitHub user data for {id}: {ex.Message}", ex);
        }
    }
}
