using ControlSpark.WebMvc.Models;
using System.Diagnostics;

namespace ControlSpark.WebMvc.Controllers
{
    public class HomeController(ILogger<BaseController> logger) : BaseController(logger)
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}