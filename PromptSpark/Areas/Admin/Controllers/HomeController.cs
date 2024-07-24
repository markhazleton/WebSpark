namespace PromptSpark.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminBaseController(WebSpark.UserIdentity.Data.WebSparkUserContext _context, UserManager<WebSpark.UserIdentity.Data.WebSparkUser> _userManager, RoleManager<IdentityRole> _roleManager) : Controller
{
}


public class HomeController(WebSpark.UserIdentity.Data.WebSparkUserContext _context, UserManager<WebSpark.UserIdentity.Data.WebSparkUser> _userManager, RoleManager<IdentityRole> _roleManager) : AdminBaseController(_context, _userManager, _roleManager)
{
    public IActionResult Index()
    {
        return View();
    }
}
