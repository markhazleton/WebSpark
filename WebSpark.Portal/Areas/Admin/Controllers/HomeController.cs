namespace WebSpark.Portal.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminBaseController(
    WebSparkUserContext _context, 
    UserManager<WebSparkUser> _userManager, 
    RoleManager<IdentityRole> _roleManager) : Controller
{
}

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
