using PromptSpark.Domain.Service;
using WebSpark.RecipeManager.Interfaces;
using WebSpark.RecipeManager.Models;

namespace WebSpark.Portal.Areas.RecipeSpark.Controllers;


/// <summary>
/// HomeController 
/// </summary>
public class HomeController(
    ILogger<HomeController> _logger,
    IRecipeService _RecipeService,
    IRecipeGPTService recipeGPTService) : RecipeBaseController
{

    // GET: RecipeListController
    public ActionResult Index()
    {
        _logger.LogInformation("Recipe List Controller Index");
        var recipes = _RecipeService.Get().ToList();
        return View(recipes);
    }

    // GET: RecipeListController/Details/5
    public async Task<ActionResult> Details(int id)
    {
        return View(_RecipeService.Get(id));
    }
    public ActionResult MomCreate()
    {
        var model = _RecipeService.Get(0);
        return View(model);
    }

    /// <summary>
    /// POST: RecipeListController/Create
    /// </summary>
    /// <param name="recipeModel"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> MomCreate(RecipeModel recipeModel)
    {
        try
        {
            var categoryList = _RecipeService.GetRecipeCategoryList();
            var category = categoryList.Where(w => w.Id == recipeModel.RecipeCategoryID).FirstOrDefault();
            var genRecipe = await recipeGPTService.CreateMomGPTRecipe(recipeModel.Name, category?.Name ?? "Main Course");
            genRecipe.DomainID = 2; // Mechanics of Motherhood Need to Pull from Config
            category = categoryList.Where(w => w.Name == genRecipe.RecipeCategoryNM).FirstOrDefault();
            if (category != null)
            {
                genRecipe.RecipeCategoryID = category.Id;
            }
            else
            {
                category = categoryList.Where(w => w.Name == "Main Course").FirstOrDefault();
                genRecipe.RecipeCategoryID = category.Id;
                genRecipe.RecipeCategoryNM = category.Name;
                genRecipe.RecipeCategory = category;
            }
            var saveResult = _RecipeService.Save(genRecipe);
            // return to edit the new recipe
            return RedirectToAction("Edit", new { id = genRecipe.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Saving Recipe with Create Method");
        }
        return RedirectToAction("List");
    }
    public ActionResult Create()
    {
        var model = _RecipeService.Get(0);
        return View(model);
    }

    /// <summary>
    /// POST: RecipeListController/Create
    /// </summary>
    /// <param name="recipeModel"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(RecipeModel recipeModel)
    {
        recipeModel.DomainID = 2; // Mechanics of Motherhood Need to Pull from Config
        try
        {
            var saveResult = _RecipeService.Save(recipeModel);

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Saving Recipe with Create Method");
            return View();
        }
    }

    // GET: RecipeListController/Edit/5
    public async Task<ActionResult> Edit(int id)
    {
        var rec = _RecipeService.Get(id);
        return View(rec);
    }

    // POST: RecipeListController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, RecipeModel item)
    {
        try
        {
            var RecipeToUpdate = _RecipeService.Get().Where(w => w.Id == id).FirstOrDefault();
            if (RecipeToUpdate != null)
            {
                RecipeToUpdate.RecipeCategoryID = item.RecipeCategoryID;
                RecipeToUpdate.AuthorNM = item.AuthorNM;
                RecipeToUpdate.Description = item.Description;
                RecipeToUpdate.Name = item.Name;
                RecipeToUpdate.Servings = item.Servings;
                RecipeToUpdate.Ingredients = item.Ingredients;
                RecipeToUpdate.Instructions = item.Instructions;
                var saveResult = _RecipeService.Save(RecipeToUpdate);
            }
            return RedirectToAction("Details", "RecipeCategory", new { id = RecipeToUpdate.RecipeCategoryID });
        }
        catch
        {
            return View();
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Delete(int id)
    {
        try
        {
            _RecipeService.Delete(id);
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }
}
