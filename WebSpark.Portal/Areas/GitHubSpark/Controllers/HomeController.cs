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
            return View("Index", userData);
        }
        catch (GitHubApiException ex)
        {
            _logger.LogError(ex, "Error retrieving GitHub user data for {Username}", username);
            return View("Error", new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                ErrorMessage = $"GitHub API error: {ex.Message}",
                StatusCode = ex.StatusCode
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
                ErrorMessage = $"GitHub API error: {ex.Message}",
                StatusCode = ex.StatusCode,
                RateLimitInfo = ex.RateLimitInfo.ToString()
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
}
