using System.Diagnostics;

namespace WebSpark.Portal.Areas.TriviaSpark.Controllers;

/// <summary>
/// TriviaSparkBaseController 
/// </summary>
[Area("TriviaSpark")]
public class TriviaSparkBaseController() : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}