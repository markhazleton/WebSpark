using Microsoft.AspNetCore.Mvc;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;
using WebSpark.Portal.Areas.RecipeSpark.Controllers;

namespace WebSpark.Portal.Areas.RecipeSpark.Controllers
{
    /// <summary>
    /// Controller for Recipe Category management functionality
    /// </summary>
    public class RecipeCategoryController : RecipeBaseController
    {
        private readonly ILogger<RecipeCategoryController> _logger;
        private readonly IRecipeService _recipeService;

        public RecipeCategoryController(
            ILogger<RecipeCategoryController> logger,
            IRecipeService recipeService)
        {
            _logger = logger;
            _recipeService = recipeService;
        }

        /// <summary>
        /// Displays the list of recipe categories
        /// </summary>
        public ActionResult Index()
        {
            _logger.LogInformation("RecipeSpark RecipeCategory Controller Index");
            var categories = _recipeService.GetRecipeCategoryList();
            return View(categories);
        }

        /// <summary>
        /// Gets recipe category details by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        public ActionResult Details(int id)
        {
            var category = _recipeService.GetRecipeCategoryById(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", id);
                TempData["ErrorMessage"] = "Category not found";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        /// <summary>
        /// Opens create category form
        /// </summary>
        public ActionResult Create()
        {
            var model = new RecipeCategoryModel
            {
                IsActive = true,
                DisplayOrder = 99
            };
            return View(model);
        }

        /// <summary>
        /// Creates a new recipe category
        /// </summary>
        /// <param name="model">Category model</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RecipeCategoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Ensure URL is URL-friendly
                if (!string.IsNullOrEmpty(model.Url))
                {
                    model.Url = model.Url.ToLower()
                        .Replace(" ", "-")
                        .Replace("_", "-")
                        .Replace(".", "-");
                }
                else
                {
                    // Auto-generate URL from name
                    model.Url = model.Name.ToLower()
                        .Replace(" ", "-")
                        .Replace("_", "-")
                        .Replace(".", "-");
                }

                _recipeService.Save(model);
                _logger.LogInformation("Created new category: {CategoryName}", model.Name);
                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                TempData["ErrorMessage"] = "Failed to create category. Please try again.";
                return View(model);
            }
        }

        /// <summary>
        /// Opens edit category form
        /// </summary>
        /// <param name="id">Category ID</param>
        public ActionResult Edit(int id)
        {
            var category = _recipeService.GetRecipeCategoryById(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        /// <summary>
        /// Updates an existing recipe category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="model">Updated category model</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, RecipeCategoryModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Invalid category ID";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Ensure URL is URL-friendly
                if (!string.IsNullOrEmpty(model.Url))
                {
                    model.Url = model.Url.ToLower()
                        .Replace(" ", "-")
                        .Replace("_", "-")
                        .Replace(".", "-");
                }

                _recipeService.Save(model);
                _logger.LogInformation("Updated category: {CategoryName}", model.Name);
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID {CategoryId}", id);
                TempData["ErrorMessage"] = "Failed to update category. Please try again.";
                return View(model);
            }
        }

        /// <summary>
        /// Deletes a recipe category
        /// </summary>
        /// <param name="id">Category ID</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var category = _recipeService.GetRecipeCategoryById(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found";
                    return RedirectToAction(nameof(Index));
                }

                string categoryName = category.Name;
                _recipeService.Delete(category);
                _logger.LogInformation("Deleted category: {CategoryName}", categoryName);
                TempData["SuccessMessage"] = "Category deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID {CategoryId}", id);
                TempData["ErrorMessage"] = "Failed to delete category. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}