using System.Diagnostics;

namespace WebSpark.Portal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Terms()
    {
        return View();
    }
    public IActionResult KitchenSink()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SetTheme(string theme)
    {
        if (!string.IsNullOrEmpty(theme))
        {
            HttpContext.Session.SetString("BootswatchTheme", theme);
        }
        return Ok();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
