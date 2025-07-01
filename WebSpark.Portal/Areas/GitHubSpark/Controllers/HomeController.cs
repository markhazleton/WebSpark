using WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

namespace WebSpark.Portal.Areas.GitHubSpark.Controllers;

public class HomeController : GitHubSparkBaseController
{
    private readonly IGitHubUserService _gitHubUserService;
    private readonly IGitHubRepositoryService _gitHubRepositoryService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IGitHubUserService gitHubUserService,
        IGitHubRepositoryService gitHubRepositoryService,
        ILogger<HomeController> logger)
    {
        _gitHubUserService = gitHubUserService;
        _gitHubRepositoryService = gitHubRepositoryService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string username = "markhazleton", CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Displaying GitHub user info for {Username}", username);
            var userData = await _gitHubUserService.GetGitHubUserDataAsync(username, ct);
            ViewData["ApiSource"] = "REST API";
            return View("Index", userData);
        }
        catch (GitHubApiException ex)
        {
            _logger.LogError(ex, "Error retrieving GitHub user data for {Username}", username);
            return View("Error", new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                ErrorMessage = $"GitHub API error: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving GitHub user data for {Username}", username);
            return View("Error", new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                ErrorMessage = "An unexpected error occurred while processing your request."
            });
        }
    }

    public async Task<IActionResult> GraphQL(string username = "markhazleton", CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Displaying GitHub user info using GraphQL API for {Username}", username);
            // Use the new GraphQL-specific implementation to fetch data
            var userData = await _gitHubUserService.FetchGitHubDataViaGraphQLAsync(username, ct);

            // Mark this view as using GraphQL API source
            ViewData["ApiSource"] = "GraphQL API";

            return View("Index", userData);
        }
        catch (GitHubApiException ex)
        {
            _logger.LogError(ex, "GraphQL API error retrieving GitHub user data for {Username}", username);
            return View("Error", new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                ErrorMessage = $"GitHub GraphQL API error: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving GitHub user data via GraphQL for {Username}", username);
            return View("Error", new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                ErrorMessage = "An unexpected error occurred while processing your GraphQL request."
            });
        }
    }

    public async Task<IActionResult> RepositoryAnalysis(string userName, string repoName, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Analyzing repository {UserName}/{RepoName}", userName, repoName);
            var repoAnalysis = await _gitHubRepositoryService.GetRepositoryAnalysisAsync(userName, repoName, ct);

            if (repoAnalysis == null)
            {
                _logger.LogWarning("Repository analysis for {UserName}/{RepoName} returned null", userName, repoName);
                return NotFound($"Repository analysis for {userName}/{repoName} could not be completed.");
            }

            return View("RepositoryAnalysis", repoAnalysis);
        }
        catch (GitHubApiException ex)
        {
            _logger.LogError(ex, "GitHub API error analyzing repository {UserName}/{RepoName}", userName, repoName);
            return View("Error", new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                ErrorMessage = $"GitHub API error: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error analyzing repository {UserName}/{RepoName}", userName, repoName);
            return View("Error", new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                ErrorMessage = "An unexpected error occurred while processing your request."
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Search(string query, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Searching GitHub users with query: {Query}", query);

            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<Services.GitHub.GitHubSearchUserResult>());
            }

            // Use GraphQL to search for users
            var searchResults = await _gitHubUserService.SearchUsersViaGraphQLAsync(query, ct);

            // Mark this view as using GraphQL API source
            ViewData["ApiSource"] = "GraphQL API";
            ViewData["SearchQuery"] = query;

            return View(searchResults);
        }
        catch (GitHubApiException ex)
        {
            _logger.LogError(ex, "GraphQL API error searching GitHub users with query: {Query}", query);
            return View("Error", new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                ErrorMessage = $"GitHub GraphQL API error: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error searching GitHub users with query: {Query}", query);
            return View("Error", new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                ErrorMessage = "An unexpected error occurred while processing your search request."
            });
        }
    }
}
