namespace ControlSpark.Web.Controllers
{
    public class AdminController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return await Task.FromResult(File("~/index.html", "text/html"));
        }

    }
}
