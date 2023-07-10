using ControlSpark.RecipeManager.Interfaces;
using ControlSpark.RecipeManager.Models;
using ControlSpark.WebMvc.Areas.Recipe.Controllers;

namespace ControlSpark.WebMvc.Areas.Recipe.Controllersl;

public class RecipeImageController : RecipeBaseController
{
    private readonly IRecipeImageService _recipeImageService;

    public RecipeImageController(ILogger<MainController> logger, IRecipeService recipeService, IRecipeImageService recipeImageService)
        : base(logger, recipeService)
    {
        _recipeImageService = recipeImageService;
    }

    public IActionResult Index()
    {
        var recipeImages = _recipeImageService.GetRecipeImages();
        return View(recipeImages);
    }

    public IActionResult Details(int id)
    {
        var recipeImage = _recipeImageService.GetRecipeImageById(id);
        if (recipeImage == null)
        {
            return NotFound();
        }

        return View(recipeImage);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(RecipeImageModel recipeImageModel)
    {

        recipeImageModel.Recipe = _RecipeService.Get(1);

        if (ModelState.IsValid)
        {
            _recipeImageService.AddRecipeImage(recipeImageModel);
            return RedirectToAction(nameof(Index));
        }

        return View(recipeImageModel);
    }

    public IActionResult Edit(int id)
    {
        var recipeImage = _recipeImageService.GetRecipeImageById(id);
        if (recipeImage == null)
        {
            return NotFound();
        }

        return View(recipeImage);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, RecipeImageModel recipeImageModel)
    {
        if (id != recipeImageModel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _recipeImageService.UpdateRecipeImage(recipeImageModel);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        return View(recipeImageModel);
    }

    public IActionResult Delete(int id)
    {
        var recipeImage = _recipeImageService.GetRecipeImageById(id);
        if (recipeImage == null)
        {
            return NotFound();
        }

        return View(recipeImage);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _recipeImageService.DeleteRecipeImage(id);
        return RedirectToAction(nameof(Index));
    }
}

