using HttpClientUtility.MemoryCache;
using HttpClientUtility.RequestResult;
using WebSpark.Portal.Areas.AsyncSpark.Services.GitHub;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

public class GitHubController : AsyncSparkBaseController
{
    private readonly GitHubUserService _gitHubUserService;
    private readonly GitHubRepositoryService _gitHubRepositoryService;
    public GitHubController(
    IHttpRequestResultService httpClientSendService,
    IMemoryCacheManager _memoryCacheManager,
    IConfiguration configuration)
    {
        var _token = configuration["GitHubPAT"] ?? "MISSING";
        _gitHubUserService = new GitHubUserService(httpClientSendService, _memoryCacheManager, _token);
        _gitHubRepositoryService = new GitHubRepositoryService(httpClientSendService, _memoryCacheManager, _token);
    }
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        return View("Index", await _gitHubUserService.GetGitHubUserDataAsync("markhazleton", ct));
    }
    public async Task<IActionResult> RepositoryAnalysis(string userName, string repoName, CancellationToken ct = default)
    {
        var repoAnalysis = await _gitHubRepositoryService.GetRepositoryAnalysisAsync(userName, repoName, ct);
        if (repoAnalysis == null)
        {
            return NotFound($"Repository analysis for {userName}/{repoName} could not be completed.");
        }
        return View("RepositoryAnalysis", repoAnalysis);
    }


}
