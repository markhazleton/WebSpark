using Microsoft.AspNetCore.Authorization;
using WebSpark.RecipeManager.Interfaces;

namespace WebSpark.WebMvc.Areas.Recipe.Controllers;

/// <summary>
/// RecipeBaseController 
/// </summary>
/// <remarks>
/// RecipeBaseController Constructor
/// </remarks>
/// <param name="logger"></param>
/// <param name="RecipeService"></param>

[Authorize]
[Area("Recipe")]
public class RecipeBaseController(ILogger<MainController> logger, IRecipeService RecipeService) : Controller
{
    protected readonly ILogger<MainController> _logger = logger;
    protected readonly IRecipeService _RecipeService = RecipeService;
}
