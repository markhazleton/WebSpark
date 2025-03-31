using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;

namespace WebSpark.Portal.Areas.WebCMS.Controllers
{
    /// <summary>
    /// Home controller for the WebSpark CMS area - provides dashboard and overview functionality
    /// </summary>
    public class HomeController : WebCMSBaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebsiteService _websiteService;
        private readonly IMenuService _menuService;
        private readonly IScopeInformation _scopeInfo;

        /// <summary>
        /// Constructor for HomeController
        /// </summary>
        public HomeController(
            ILogger<HomeController> logger,
            IWebsiteService websiteService,
            IMenuService menuService,
            IScopeInformation scopeInfo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _websiteService = websiteService ?? throw new ArgumentNullException(nameof(websiteService));
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _scopeInfo = scopeInfo ?? throw new ArgumentNullException(nameof(scopeInfo));
        }

        /// <summary>
        /// Displays the dashboard with summary information and quick access to common tasks
        /// </summary>
        public IActionResult Index()
        {
            try
            {
                // Get summary information for the dashboard
                var websiteCount = 0;
                var menuItemCount = 0;
                var recentWebsites = Array.Empty<WebsiteModel>();
                var recentMenuItems = Array.Empty<MenuModel>();

                try
                {
                    // Get website information
                    var websites = _websiteService.Get();
                    websiteCount = websites.Count();
                    recentWebsites = websites.OrderByDescending(w => w.ModifiedDT).Take(5).ToArray();

                    // Get menu information
                    var menuItems = _menuService.GetAllMenuItems();
                    menuItemCount = menuItems.Count();
                    recentMenuItems = menuItems.OrderByDescending(m => m.LastModified).Take(5).ToArray();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to load all dashboard information");
                }

                // Pass data to the view
                ViewData["WebsiteCount"] = websiteCount;
                ViewData["MenuItemCount"] = menuItemCount;
                ViewData["RecentWebsites"] = recentWebsites;
                ViewData["RecentMenuItems"] = recentMenuItems;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading WebSpark CMS dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return View();
            }
        }

        /// <summary>
        /// Displays help/documentation for the WebSpark CMS system
        /// </summary>
        [HttpGet]
        public IActionResult Help()
        {
            return View();
        }
    }
}