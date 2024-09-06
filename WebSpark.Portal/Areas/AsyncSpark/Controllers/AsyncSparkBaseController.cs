using System.Diagnostics;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

[Area("AsyncSpark")]
public class AsyncSparkBaseController() : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
