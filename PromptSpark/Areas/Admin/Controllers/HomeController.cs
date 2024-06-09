using PromptSpark.Areas.Identity.Data;

namespace PromptSpark.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminBaseController(AdminContext _context, UserManager<AdminUser> _userManager, RoleManager<IdentityRole> _roleManager) : Controller
{
}


public class HomeController(AdminContext _context, UserManager<AdminUser> _userManager, RoleManager<IdentityRole> _roleManager) : AdminBaseController(_context, _userManager, _roleManager)
{
    public IActionResult Index()
    {
        return View();
    }
}
