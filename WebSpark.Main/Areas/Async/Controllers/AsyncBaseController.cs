using System.Diagnostics;

namespace WebSpark.Main.Areas.Async.Controllers;

[Area("Async")]
public class AsyncBaseController() : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}