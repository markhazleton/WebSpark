using ControlSpark.RecipeManager.Interfaces;

namespace ControlSpark.WebMvc.Areas.Recipe.Controllers
{
    public class RecipeImageController : RecipeBaseController
    {
        public RecipeImageController(ILogger<MainController> logger, IRecipeService RecipeService) : base(logger, RecipeService)
        {
        }

        public IActionResult Index()
        {
            return View(_RecipeService.GetRecipeImages());
        }
    }
}
