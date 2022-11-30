﻿using ControlSpark.RecipeManager.Interfaces;
using ControlSpark.RecipeManager.Models;

namespace ControlSpark.WebMvc.Areas.Recipe.Controllers;


/// <summary>
/// MainController 
/// </summary>
[Area("Recipe")]
public class MainController : RecipeBaseController
{
    public MainController(ILogger<MainController> logger, IRecipeService RecipeService) : base(logger, RecipeService)
    {
    }

    // GET: RecipeListController
    public ActionResult Index()
    {
        return View(_RecipeService.Get());
    }

    // GET: RecipeListController/Details/5
    public async Task<ActionResult> Details(int id)
    {
        return View(_RecipeService.Get(id));
    }

    // GET: RecipeListController/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: RecipeListController/Create
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
                RecipeToUpdate.Ingredients = item.Ingredients;
                RecipeToUpdate.Instructions = item.Instructions;
                var saveResult = _RecipeService.Save(RecipeToUpdate);
            }
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: RecipeListController/Delete/5
    public ActionResult Delete(int id)
    {
        return View();
    }

    // POST: RecipeListController/Delete/5
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