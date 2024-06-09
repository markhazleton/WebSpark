using PromptSpark.Models;
using System.Diagnostics;

namespace PromptSpark.Areas.OpenAI.Controllers;

[Area("OpenAI")]
public class OpenAIBaseController() : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
