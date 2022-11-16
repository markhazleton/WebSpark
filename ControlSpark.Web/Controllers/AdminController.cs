namespace ControlSpark.Web.Controllers
{
    public class AdminController : BaseController
    {
        public AdminController(
            ILogger<AdminController> logger,
            IConfiguration configuration,
            IWebsiteService websiteService) : base(logger, configuration, websiteService)
        {
        }
        /// <summary>
        /// Admin Home Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View(BaseVM);
        }
    }
}
