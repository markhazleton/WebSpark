namespace WebSpark.Portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminBaseController(
        WebSparkUserContext _context,
        UserManager<WebSparkUser> _userManager,
        RoleManager<IdentityRole> _roleManager) : Controller
    {
    }
}
