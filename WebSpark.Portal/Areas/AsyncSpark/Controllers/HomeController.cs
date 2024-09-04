namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

public class HomeController : AsyncSparkBaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
