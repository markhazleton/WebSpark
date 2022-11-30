using ControlSpark.RecipeManager.Interfaces;

namespace ControlSpark.WebMvc.Areas.Recipe.Controllers;

/// <summary>
/// RecipeCategoryController 
/// </summary>
[Area("Recipe")]
public class RecipeCategoryController : RecipeBaseController
{
    /// <summary>
    /// RecipeCategoryController 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="RecipeService"></param>
    public RecipeCategoryController(ILogger<MainController> logger, IRecipeService RecipeService) : base(logger, RecipeService)
    {
    }

    // GET: RecipeCategoryController
    public ActionResult Index()
    {
        return View(_RecipeService.GetRecipeCategoryList());
    }

    // GET: RecipeCategoryController/Details/5
    public ActionResult Details(int id)
    {
        return View();
    }

    // GET: RecipeCategoryController/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: RecipeCategoryController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: RecipeCategoryController/Edit/5
    public ActionResult Edit(int id)
    {
        return View();
    }

    // POST: RecipeCategoryController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: RecipeCategoryController/Delete/5
    public ActionResult Delete(int id)
    {
        return View();
    }

    // POST: RecipeCategoryController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Delete(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }
}
