using System.Diagnostics;

namespace WebSpark.Portal.Areas.PromptSpark.Controllers;

[Area("PromptSpark")]
public class PromptSparkBaseController() : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
