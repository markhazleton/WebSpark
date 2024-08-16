namespace WebSpark.Main.Areas.Async.Controllers;

public class HomeController : AsyncBaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
