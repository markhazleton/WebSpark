namespace WebSpark.Main.Areas.DataSpark.Controllers;

public class HomeController : DataSparkBaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
