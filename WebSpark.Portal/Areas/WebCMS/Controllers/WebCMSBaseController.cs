using System.Diagnostics;

namespace WebSpark.Portal.Areas.WebCMS.Controllers;

[Area("WebCMS")]
[Authorize]
public class WebCMSBaseController() : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
