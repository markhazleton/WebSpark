using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebSpark.HttpClientUtility.MemoryCache;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.Portal.Areas.GitHubSpark.Models;

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
        cachedData.Followers?.ErrorList?.Add(errorMessage);
        cachedData.Following?.ErrorList?.Add(errorMessage);
        _logger.LogWarning("Rate limit exceeded: {ErrorMessage}", errorMessage);
    }

    private HttpRequestResult<T> CreateGitHubRequest<T>(string requestPath)
    {
        var request = new HttpRequestResult<T>
        {
            CacheDurationMinutes = _options.DefaultCacheDurationMinutes,
            RequestPath = requestPath
        };
        request.RequestHeaders?.Add("User-Agent", _options.UserAgent);
        request.RequestHeaders?.Add("Authorization", $"token {_options.PersonalAccessToken}");

        // Add accept header for GitHub API v3
        request.RequestHeaders?.Add("Accept", "application/vnd.github.v3+json");

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

    /// <inheritdoc/>
    public async Task<GitHubCacheViewModel> FetchGitHubDataViaGraphQLAsync(string userName, CancellationToken ct)
    {
        _logger.LogInformation("Fetching GitHub data via GraphQL for user {UserName}", userName);

        try
        {
            // The GraphQL query to fetch user data, repositories, followers, and following
            var graphQLQuery = @"
            query ($login: String!) {
              user(login: $login) {
                login
                name
                bio
                avatarUrl
                url
                company
                location
                email
                websiteUrl
                twitterUsername
                createdAt
                updatedAt
                repositories(first: 30, orderBy: {field: UPDATED_AT, direction: DESC}) {
                  totalCount
                  nodes {
                    name
                    description
                    url
                    stargazerCount
                    forkCount
                    isPrivate
                    primaryLanguage {
                      name
                      color
                    }
                    updatedAt
                  }
                }
                followers(first: 20) {
                  totalCount
                  nodes {
                    login
                    name
                    avatarUrl
                    url
                  }
                }
                following(first: 20) {
                  totalCount
                  nodes {
                    login
                    name
                    avatarUrl
                    url
                  }
                }
              }
            }";

            // Create the request body
            var requestBody = new
            {
                query = graphQLQuery,
                variables = new { login = userName }
            };

            // Serialize the request body
            var jsonContent = JsonSerializer.Serialize(requestBody);

            // Create the GraphQL request
            var requestPath = "https://api.github.com/graphql";
            var request = new HttpRequestResult<GraphQLResponse>
            {
                CacheDurationMinutes = _options.DefaultCacheDurationMinutes,
                RequestPath = requestPath,
            };

            // Add headers
            request.RequestHeaders?.Add("User-Agent", _options.UserAgent);
            request.RequestHeaders?.Add("Authorization", $"bearer {_options.PersonalAccessToken}");
            request.RequestHeaders?.Add("Content-Type", "application/json");

            // Send the request with POST method and JSON content
            _logger.LogDebug("Sending GitHub GraphQL API request to {RequestPath}", requestPath);

            // Create an HttpClient for the GraphQL request
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(_options.UserAgent);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _options.PersonalAccessToken);

            // Send the request
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var httpResponse = await httpClient.PostAsync(requestPath, httpContent, ct);
            var responseContent = await httpResponse.Content.ReadAsStringAsync(ct);

            // Deserialize the response
            var response = JsonSerializer.Deserialize<GraphQLResponse>(responseContent);

            if (!httpResponse.IsSuccessStatusCode || response == null || response.Data == null)
            {
                var errorMessage = response?.Errors != null && response.Errors.Count > 0
                    ? string.Join(", ", response.Errors.Select(e => e.Message))
                    : "Unknown GraphQL error";

                throw new GitHubApiException($"Failed to fetch data from GitHub GraphQL API: {errorMessage}",
                    null, (int)httpResponse.StatusCode);
            }

            // Map the GraphQL response to our view model
            var graphQLData = response.Data;
            if (graphQLData.User == null)
            {
                throw new GitHubApiException($"User {userName} not found", null, 404);
            }

            // Create HttpRequestResult objects for each component
            var userResult = CreateResultWithContent(MapToGitHubUser(graphQLData.User), _options.DefaultCacheDurationMinutes);
            var repoResult = CreateResultWithContent(MapToGitHubRepos(graphQLData.User.Repositories), _options.DefaultCacheDurationMinutes);
            var followersResult = CreateResultWithContent(MapToGitHubFollowers(graphQLData.User.Followers), _options.DefaultCacheDurationMinutes);
            var followingResult = CreateResultWithContent(MapToGitHubFollowers(graphQLData.User.Following), _options.DefaultCacheDurationMinutes);

            var result = new GitHubCacheViewModel
            {
                UserName = userName,
                LastUpdated = DateTime.UtcNow,
                User = userResult,
                RepoInfo = repoResult,
                Followers = followersResult,
                Following = followingResult
            };

            _logger.LogInformation("Successfully fetched GitHub data via GraphQL for {UserName}", userName);
            return result;
        }
        catch (GitHubApiException)
        {
            throw; // Re-throw GitHubApiExceptions without wrapping
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FetchGitHubDataViaGraphQLAsync for user {UserName}", userName);
            throw new GitHubApiException($"Failed to fetch GitHub data via GraphQL for user {userName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates an HttpRequestResult with the specified content and cache duration
    /// </summary>
    private HttpRequestResult<T> CreateResultWithContent<T>(T content, int cacheDurationMinutes)
    {
        var result = new HttpRequestResult<T>
        {
            RequestPath = string.Empty,
            StatusCode = HttpStatusCode.OK,
            CacheDurationMinutes = cacheDurationMinutes,
            ErrorList = new List<string>()
        };

        // Set the content through reflection since the Content property may not be directly accessible
        typeof(HttpRequestResult<T>)
            .GetProperty("Content")?
            .SetValue(result, content);

        return result;
    }

    private GitHubUser MapToGitHubUser(GraphQLUser user)
    {
        return new GitHubUser
        {
            Login = user.Login ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Bio = user.Bio ?? string.Empty,
            AvatarUrl = user.AvatarUrl ?? string.Empty,
            HtmlUrl = user.Url ?? string.Empty,
            Company = user.Company ?? string.Empty,
            Location = user.Location ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Blog = user.WebsiteUrl ?? string.Empty,
            TwitterUsername = user.TwitterUsername ?? string.Empty,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            PublicRepos = user.Repositories?.TotalCount ?? 0
        };
    }

    private List<GitHubRepo> MapToGitHubRepos(GraphQLRepositories? repositories)
    {
        if (repositories?.Nodes == null)
            return new List<GitHubRepo>();

        return repositories.Nodes.Select(repo => new GitHubRepo
        {
            Name = repo.Name ?? string.Empty,
            Description = repo.Description ?? string.Empty,
            HtmlUrl = repo.Url ?? string.Empty,
            StargazersCount = repo.StargazerCount,
            ForksCount = repo.ForkCount,
            Private = repo.IsPrivate,
            Language = repo.PrimaryLanguage?.Name ?? string.Empty,
            UpdatedAt = repo.UpdatedAt
        }).ToList();
    }

    private List<GitHubFollower> MapToGitHubFollowers(GraphQLFollowers? followers)
    {
        if (followers?.Nodes == null)
            return new List<GitHubFollower>();

        return followers.Nodes.Select(follower => new GitHubFollower
        {
            Login = follower.Login ?? string.Empty,
            AvatarUrl = follower.AvatarUrl ?? string.Empty,
            HtmlUrl = follower.Url ?? string.Empty
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<GitHubSearchUserResult>> SearchUsersViaGraphQLAsync(string searchQuery, CancellationToken ct)
    {
        _logger.LogInformation("Searching GitHub users via GraphQL for query: {SearchQuery}", searchQuery);

        try
        {
            // The GraphQL query to search for users
            var graphQLQuery = @"
            query ($searchQuery: String!) {
              search(query: $searchQuery, type: USER, first: 10) {
                userCount
                edges {
                  node {
                    ... on User {
                      login
                      name
                      avatarUrl
                      url
                      bio
                      repositories {
                        totalCount
                      }
                    }
                  }
                }
              }
            }";

            // Create the request body
            var requestBody = new
            {
                query = graphQLQuery,
                variables = new { searchQuery = searchQuery }
            };

            // Serialize the request body
            var jsonContent = JsonSerializer.Serialize(requestBody);

            // Create an HttpClient for the GraphQL request
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(_options.UserAgent);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _options.PersonalAccessToken);

            // Send the request
            var requestPath = "https://api.github.com/graphql";
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var httpResponse = await httpClient.PostAsync(requestPath, httpContent, ct);
            var responseContent = await httpResponse.Content.ReadAsStringAsync(ct);

            // Deserialize the response
            var response = JsonSerializer.Deserialize<GraphQLResponse>(responseContent);

            if (!httpResponse.IsSuccessStatusCode || response == null || response.Data == null || response.Data.Search == null)
            {
                var errorMessage = response?.Errors != null && response.Errors.Count > 0
                    ? string.Join(", ", response.Errors.Select(e => e.Message))
                    : "Unknown GraphQL error";

                throw new GitHubApiException($"Failed to search GitHub users via GraphQL: {errorMessage}",
                    null, (int)httpResponse.StatusCode);
            }

            var searchResults = new List<GitHubSearchUserResult>();

            if (response.Data.Search.Edges != null)
            {
                foreach (var edge in response.Data.Search.Edges)
                {
                    if (edge.Node != null)
                    {
                        searchResults.Add(new GitHubSearchUserResult
                        {
                            Login = edge.Node.Login ?? string.Empty,
                            Name = edge.Node.Name,
                            AvatarUrl = edge.Node.AvatarUrl,
                            Url = edge.Node.Url,
                            Bio = edge.Node.Bio,
                            RepositoryCount = edge.Node.Repositories?.TotalCount ?? 0
                        });
                    }
                }
            }

            _logger.LogInformation("Successfully searched GitHub users via GraphQL. Found {Count} results for query: {SearchQuery}",
                searchResults.Count, searchQuery);

            return searchResults;
        }
        catch (GitHubApiException)
        {
            throw; // Re-throw GitHubApiExceptions without wrapping
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchUsersViaGraphQLAsync for query: {SearchQuery}", searchQuery);
            throw new GitHubApiException($"Failed to search GitHub users via GraphQL for query '{searchQuery}': {ex.Message}", ex);
        }
    }
}
