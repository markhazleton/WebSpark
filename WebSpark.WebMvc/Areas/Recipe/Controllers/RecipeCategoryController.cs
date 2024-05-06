using WebSpark.RecipeManager.Interfaces;
using WebSpark.RecipeManager.Models;

namespace WebSpark.WebMvc.Areas.Recipe.Controllers;

/// <summary>
/// RecipeCategoryController 
/// </summary>
/// <remarks>
/// RecipeCategoryController 
/// </remarks>
/// <param name="_logger"></param>
/// <param name="_RecipeService"></param>
public class RecipeCategoryController(ILogger<MainController> _logger, IRecipeService _RecipeService) : RecipeBaseController
{

    // GET: RecipeCategoryController
    public ActionResult Index()
    {
        return View(_RecipeService.GetRecipeCategoryList());
    }

    // GET: RecipeCategoryController/Details/5
    public ActionResult Details(int id)
    {
        var item = _RecipeService.GetRecipeCategoryById(id);
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
