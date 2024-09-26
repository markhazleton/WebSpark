using System.Text.Json;
using TriviaSpark.Domain.Services;

namespace WebSpark.Portal.Areas.TriviaSpark.Controllers;

public class HomeController : TriviaSparkBaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
