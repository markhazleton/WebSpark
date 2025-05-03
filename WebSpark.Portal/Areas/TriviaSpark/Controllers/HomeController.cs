using TriviaSpark.JShow.Service;
using WebSpark.HttpClientUtility.MemoryCache;

namespace WebSpark.Portal.Areas.TriviaSpark.Controllers;

public class HomeController : TriviaSparkBaseController
{
    public HomeController(IMemoryCacheManager memoryCacheManager, IJShowService jShowService) : base(memoryCacheManager, jShowService)
    {
    }

    public IActionResult Index()
    {
        return View();
    }
}
