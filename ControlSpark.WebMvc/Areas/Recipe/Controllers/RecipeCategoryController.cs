using ControlSpark.RecipeManager.Interfaces;
using ControlSpark.RecipeManager.Models;

namespace ControlSpark.WebMvc.Areas.Recipe.Controllers;

/// <summary>
/// RecipeCategoryController 
/// </summary>
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
        var item = _RecipeService.GetRecipeCategoryList().Where(w => w.Id == id).FirstOrDefault();
        if (item is null)
            return RedirectToAction(nameof(Index));

        return View(item);
    }

    // GET: RecipeCategoryController/Create
    public ActionResult Create()
    {
        return View(new RecipeCategoryModel());
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
        var item = _RecipeService.GetRecipeCategoryList().Where(w => w.Id == id).FirstOrDefault();
        if (item is null)
            return RedirectToAction(nameof(Index));

        return View(item);
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
