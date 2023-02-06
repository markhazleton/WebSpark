using ControlSpark.RecipeManager.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ControlSpark.WebMvc.Areas.Recipe.Controllers;

/// <summary>
/// RecipeBaseController 
/// </summary>

[Authorize]
[Area("Recipe")]
public class RecipeBaseController : Controller
{
    protected readonly ILogger<MainController> _logger;
    protected readonly IRecipeService _RecipeService;

    /// <summary>
    /// RecipeBaseController Constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="RecipeService"></param>
    public RecipeBaseController(ILogger<MainController> logger, IRecipeService RecipeService)
    {
        _RecipeService = RecipeService;
        _logger = logger;
    }
}
