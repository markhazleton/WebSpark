using System.Diagnostics;

namespace WebSpark.Portal.Areas.RecipeSpark.Controllers;

/// <summary>
/// RecipeBaseController 
/// </summary>
/// <remarks>
/// RecipeBaseController Constructor
/// </remarks>
[Area("RecipeSpark")]
[Authorize]
public class RecipeBaseController() : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
