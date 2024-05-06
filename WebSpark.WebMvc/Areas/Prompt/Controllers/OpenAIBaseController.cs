using System.Diagnostics;
using WebSpark.Prompt.Models;

namespace WebSpark.WebMvc.Areas.Prompt.Controllers;

[Area("Prompt")]
public class OpenAIBaseController() : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
