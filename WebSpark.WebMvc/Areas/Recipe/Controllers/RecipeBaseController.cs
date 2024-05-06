using Microsoft.AspNetCore.Authorization;

namespace WebSpark.WebMvc.Areas.Recipe.Controllers;

/// <summary>
/// RecipeBaseController 
/// </summary>
/// <remarks>
/// RecipeBaseController Constructor
/// </remarks>
[Authorize]
[Area("Recipe")]
public class RecipeBaseController() : Controller
{
}
