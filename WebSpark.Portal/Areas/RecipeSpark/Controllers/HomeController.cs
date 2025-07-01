using PromptSpark.Domain.Service;
using System;
using System.Linq;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;

namespace WebSpark.Portal.Areas.RecipeSpark.Controllers
{
    /// <summary>
    /// Controller for Recipe management functionality
    /// </summary>
    public class HomeController : RecipeBaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRecipeService _recipeService;
        private readonly IRecipeGPTService _recipeGPTService;

        public HomeController(
            ILogger<HomeController> logger,
            IRecipeService recipeService,
            IRecipeGPTService recipeGPTService)
        {
            _logger = logger;
            _recipeService = recipeService;
            _recipeGPTService = recipeGPTService;
        }

        /// <summary>
        /// Displays the list of recipes
        /// </summary>
        public ActionResult Index()
        {
            _logger.LogInformation("RecipeSpark Home Controller Index");
            var recipes = _recipeService.Get().ToList();
            return View(recipes);
        }

        /// <summary>
        /// Gets recipe detail by ID
        /// </summary>
        /// <param name="id">Recipe ID</param>
        public async Task<ActionResult> Details(int id)
        {
            var recipe = _recipeService.Get(id);
            if (recipe == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(recipe);
        }

        /// <summary>
        /// Opens AI recipe generation form
        /// </summary>
        public ActionResult MomCreate()
        {
            var model = _recipeService.Get(0);
            return View(model);
        }

        /// <summary>
        /// Generates a recipe using AI
        /// </summary>
        /// <param name="recipeModel">Recipe input model</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MomCreate(RecipeModel recipeModel)
        {
            try
            {
                var model = _recipeService.Get(0);
                var categoryList = _recipeService.GetRecipeCategoryList();
                var category = categoryList.FirstOrDefault(w => w.Id == recipeModel.RecipeCategoryID);

                var genRecipe = await _recipeGPTService.CreateMomGPTRecipe(
                    model,
                    recipeModel.Name,
                    category?.Name ?? "Main Course");

                genRecipe.DomainID = 2; // Mechanics of Motherhood - TODO: Pull from Config

                // Handle recipe category
                category = categoryList.FirstOrDefault(w => w.Name == genRecipe.RecipeCategoryNM);
                if (category != null)
                {
                    genRecipe.RecipeCategoryID = category.Id;
                }
                else
                {
                    category = categoryList.FirstOrDefault(w => w.Name == "Main Course");
                    genRecipe.RecipeCategoryID = category.Id;
                    genRecipe.RecipeCategoryNM = category.Name;
                    genRecipe.RecipeCategory = category;
                }

                var saveResult = _recipeService.Save(genRecipe);

                // Redirect to Edit with the new Recipe ID
                return RedirectToAction("Edit", new { id = saveResult.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI recipe");
                TempData["ErrorMessage"] = "Failed to generate recipe. Please try again.";
                return View(recipeModel);
            }
        }

        /// <summary>
        /// Opens create recipe form
        /// </summary>
        public ActionResult Create()
        {
            var model = _recipeService.Get(0);
            return View(model);
        }

        /// <summary>
        /// Creates a new recipe
        /// </summary>
        /// <param name="recipeModel">Recipe model</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RecipeModel recipeModel)
        {
            recipeModel.DomainID = 2; // TODO: Pull from Config

            try
            {
                var saveResult = _recipeService.Save(recipeModel);
                TempData["SuccessMessage"] = "Recipe created successfully!";
                return RedirectToAction(nameof(Details), new { id = saveResult.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recipe");
                TempData["ErrorMessage"] = "Failed to create recipe. Please try again.";
                return View(recipeModel);
            }
        }

        /// <summary>
        /// Opens edit recipe form
        /// </summary>
        /// <param name="id">Recipe ID</param>
        public async Task<ActionResult> Edit(int id)
        {
            var recipe = _recipeService.Get(id);
            if (recipe == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(recipe);
        }

        /// <summary>
        /// Updates an existing recipe
        /// </summary>
        /// <param name="id">Recipe ID</param>
        /// <param name="item">Updated recipe model</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, RecipeModel item)
        {
            try
            {
                var recipeToUpdate = _recipeService.Get(id); 
                if (recipeToUpdate == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                // Update recipe properties
                recipeToUpdate.RecipeCategoryID = item.RecipeCategoryID;
                recipeToUpdate.AuthorNM = item.AuthorNM;
                recipeToUpdate.Description = string.IsNullOrWhiteSpace(item.Description) ? " " : item.Description;
                recipeToUpdate.Name = item.Name;
                recipeToUpdate.Servings = item.Servings;
                recipeToUpdate.Ingredients = item.Ingredients;
                recipeToUpdate.Instructions = item.Instructions;

                var saveResult = _recipeService.Save(recipeToUpdate);
                TempData["SuccessMessage"] = "Recipe updated successfully!";

                return RedirectToAction("Details", "RecipeCategory", new { id = recipeToUpdate.RecipeCategoryID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recipe");
                TempData["ErrorMessage"] = "Failed to update recipe. Please try again.";
                return View(item);
            }
        }

        /// <summary>
        /// Deletes a recipe
        /// </summary>
        /// <param name="id">Recipe ID</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                _recipeService.Delete(id);
                TempData["SuccessMessage"] = "Recipe deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recipe");
                TempData["ErrorMessage"] = "Failed to delete recipe. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}