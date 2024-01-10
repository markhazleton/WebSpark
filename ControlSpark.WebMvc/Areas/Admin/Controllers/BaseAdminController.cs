using Microsoft.AspNetCore.Authorization;

namespace ControlSpark.WebMvc.Areas.Admin.Controllers;

/// <summary>
/// Base Admin Controller
/// </summary>
[Authorize]
[Area("Admin")]
public class BaseAdminController : Controller
{

}
