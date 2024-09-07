namespace WebSpark.Portal.Areas.Admin.Controllers;

public class HomeController(
    WebSparkUserContext _context,
    UserManager<WebSparkUser> _userManager,
    RoleManager<IdentityRole> _roleManager) : AdminBaseController(_context, _userManager, _roleManager)
{
    public IActionResult Index()
    {
        return View();
    }
}
