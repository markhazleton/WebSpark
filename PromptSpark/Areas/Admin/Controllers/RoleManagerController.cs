using PromptSpark.Areas.Identity.Data;

namespace PromptSpark.Areas.Admin.Controllers;

public class RoleManagerController(AdminContext _context, UserManager<AdminUser> _userManager, RoleManager<IdentityRole> _roleManager) : AdminBaseController(_context, _userManager, _roleManager)
{
    public async Task<IActionResult> Index()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return View(roles);
    }
    [HttpPost]
    public async Task<IActionResult> AddRole(string roleName)
    {
        if (roleName != null)
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
        }
        return RedirectToAction("Index");
    }
}
