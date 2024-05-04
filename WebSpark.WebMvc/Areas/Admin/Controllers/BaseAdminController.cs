using Microsoft.AspNetCore.Authorization;

namespace WebSpark.WebMvc.Areas.Admin.Controllers;

/// <summary>
/// Base Admin Controller
/// </summary>
[Authorize]
[Area("Admin")]
public class BaseAdminController : Controller
{

}
