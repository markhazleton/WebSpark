namespace WebSpark.Portal.Areas.Admin.Controllers;

public class UserRolesController(
    WebSparkUserContext _context, 
    UserManager<WebSparkUser> _userManager, 
    RoleManager<IdentityRole> _roleManager) : 
    AdminBaseController(_context, _userManager, _roleManager)
{
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userRolesViewModel = new List<UserRolesViewModel>();
        foreach (WebSparkUser user in users)
        {
            var thisViewModel = new UserRolesViewModel();
            thisViewModel.UserId = user.Id;
            thisViewModel.Email = user.Email;
            thisViewModel.FirstName = user.FirstName;
            thisViewModel.LastName = user.LastName;
            thisViewModel.Roles = await GetUserRoles(user);
            userRolesViewModel.Add(thisViewModel);
        }
        return View(userRolesViewModel);
    }
    public async Task<IActionResult> Manage(string userId)
    {
        ViewBag.userId = userId;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            ViewBag.ErrorMessage = $"User with ServiceDefintionId = {userId} cannot be found";
            return View("NotFound");
        }
        ViewBag.UserName = user.UserName;
        var model = new List<ManageUserRolesViewModel>();
        foreach (var role in _roleManager.Roles)
        {
            var userRolesViewModel = new ManageUserRolesViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name
            };
            if (await _userManager.IsInRoleAsync(user, role.Name))
            {
                userRolesViewModel.Selected = true;
            }
            else
            {
                userRolesViewModel.Selected = false;
            }
            model.Add(userRolesViewModel);
        }
        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return View();
        }
        var roles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.RemoveFromRolesAsync(user, roles);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Cannot remove user existing roles");
            return View(model);
        }
        result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Cannot add selected roles to user");
            return View(model);
        }
        return RedirectToAction("Index");
    }
    private async Task<List<string>> GetUserRoles(WebSparkUser user)
    {
        return new List<string>(await _userManager.GetRolesAsync(user));
    }
}