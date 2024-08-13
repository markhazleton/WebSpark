using System.Diagnostics;

namespace PromptSpark.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    IWebHostEnvironment env) : Controller
{
    public IActionResult KitchenSink()
    {
        var filePath = Path.Combine(env.WebRootPath, "kitchen_sink.html");
        var htmlContent = System.IO.File.ReadAllText(filePath);
        return View((object)htmlContent);
    }

    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
