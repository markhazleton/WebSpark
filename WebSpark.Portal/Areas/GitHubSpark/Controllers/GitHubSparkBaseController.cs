using System.Diagnostics;

namespace WebSpark.Portal.Areas.GitHubSpark.Controllers;

/// <summary>
/// Base controller for all GitHubSpark controllers
/// </summary>
[Area("GitHubSpark")]
public class GitHubSparkBaseController : Controller
{
    /// <summary>
    /// Handles error requests
    /// </summary>
    /// <returns>Error view</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ErrorMessage = "An error occurred while processing your request."
        });
    }
}