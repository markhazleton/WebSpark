using System.Diagnostics;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers.Api;

/// <summary>
/// Base for all Api Controllers in this project
/// </summary>
[Produces("application/json")]
[ApiController]
[Area("AsyncSpark")]
public abstract class BaseAsyncSparkApiController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
