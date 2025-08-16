using Microsoft.AspNetCore.Mvc;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace WebSpark.Portal.Areas.RecipeSpark.Controllers.Api
{
    /// <summary>
    /// RESTful API Controller for Recipe and Category management
    /// </summary>
    [ApiController]
    [Route("api/recipespark")]
    [Produces("application/json")]
    public class RecipeSparkApiController : ControllerBase
    {
        private readonly ILogger<RecipeSparkApiController> _logger;
        private readonly IRecipeService _recipeService;

        public RecipeSparkApiController(
            ILogger<RecipeSparkApiController> logger,
            IRecipeService recipeService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
        }

        #region Recipe Endpoints

        /// <summary>
        /// Get all recipes with optional filtering
        /// </summary>
        /// <param name="categoryId">Optional category filter</param>
        /// <param name="searchTerm">Optional search term for name/description</param>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Page size for pagination (default: 20, max: 100)</param>
        /// <returns>List of recipes</returns>
        [HttpGet("recipes")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RecipeModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult<ApiResponse<IEnumerable<RecipeModel>>> GetRecipes(
            [FromQuery] int? categoryId = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Getting recipes with filters - CategoryId: {CategoryId}, SearchTerm: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}",
                    categoryId, searchTerm, pageNumber, pageSize);

                var allRecipes = _recipeService.Get().AsQueryable();

                // Apply category filter
                if (categoryId.HasValue)
                {
                    allRecipes = allRecipes.Where(r => r.RecipeCategoryID == categoryId.Value);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    allRecipes = allRecipes.Where(r =>
                        r.Name.ToLower().Contains(searchLower) ||
                        (r.Description != null && r.Description.ToLower().Contains(searchLower)) ||
                        (r.Ingredients != null && r.Ingredients.ToLower().Contains(searchLower)));
                }

                var totalCount = allRecipes.Count();
                var recipes = allRecipes
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new ApiResponse<IEnumerable<RecipeModel>>
                {
                    Data = recipes,
                    Success = true,
                    Message = $"Retrieved {recipes.Count} recipes",
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recipes");
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Failed to retrieve recipes",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get a specific recipe by ID
        /// </summary>
        /// <param name="id">Recipe ID</param>
        /// <returns>Recipe details</returns>
        [HttpGet("recipes/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<RecipeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public ActionResult<ApiResponse<RecipeModel>> GetRecipe(int id)
        {
            try
            {
                _logger.LogInformation("Getting recipe with ID: {RecipeId}", id);

                var recipe = _recipeService.Get(id);
                if (recipe == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Message = $"Recipe with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<RecipeModel>
                {
                    Data = recipe,
                    Success = true,
                    Message = "Recipe retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recipe with ID: {RecipeId}", id);
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Failed to retrieve recipe",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new recipe
        /// </summary>
        /// <param name="request">Recipe creation request</param>
        /// <returns>Created recipe</returns>
        [HttpPost("recipes")]
        [ProducesResponseType(typeof(ApiResponse<RecipeModel>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult<ApiResponse<RecipeModel>> CreateRecipe([FromBody] CreateRecipeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid recipe data",
                        ValidationErrors = ModelState.GetValidationErrors()
                    });
                }

                _logger.LogInformation("Creating new recipe: {RecipeName}", request.Name);

                var recipe = new RecipeModel
                {
                    Name = request.Name,
                    Description = request.Description ?? string.Empty,
                    Ingredients = request.Ingredients ?? string.Empty,
                    Instructions = request.Instructions ?? string.Empty,
                    Servings = request.Servings,
                    AuthorNM = request.AuthorName ?? string.Empty,
                    RecipeCategoryID = request.CategoryId,
                    DomainID = 2 // Mechanics of Motherhood - TODO: Pull from Config
                };

                var savedRecipe = _recipeService.Save(recipe);

                return CreatedAtAction(
                    nameof(GetRecipe),
                    new { id = savedRecipe.Id },
                    new ApiResponse<RecipeModel>
                    {
                        Data = savedRecipe,
                        Success = true,
                        Message = "Recipe created successfully"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recipe");
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Failed to create recipe",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Update an existing recipe
        /// </summary>
        /// <param name="id">Recipe ID</param>
        /// <param name="request">Recipe update request</param>
        /// <returns>Updated recipe</returns>
        [HttpPut("recipes/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<RecipeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult<ApiResponse<RecipeModel>> UpdateRecipe(int id, [FromBody] UpdateRecipeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid recipe data",
                        ValidationErrors = ModelState.GetValidationErrors()
                    });
                }

                _logger.LogInformation("Updating recipe with ID: {RecipeId}", id);

                var existingRecipe = _recipeService.Get(id);
                if (existingRecipe == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Message = $"Recipe with ID {id} not found"
                    });
                }

                // Update properties
                existingRecipe.Name = request.Name;
                existingRecipe.Description = request.Description ?? string.Empty;
                existingRecipe.Ingredients = request.Ingredients ?? string.Empty;
                existingRecipe.Instructions = request.Instructions ?? string.Empty;
                existingRecipe.Servings = request.Servings;
                existingRecipe.AuthorNM = request.AuthorName ?? string.Empty;
                existingRecipe.RecipeCategoryID = request.CategoryId;

                var updatedRecipe = _recipeService.Save(existingRecipe);

                return Ok(new ApiResponse<RecipeModel>
                {
                    Data = updatedRecipe,
                    Success = true,
                    Message = "Recipe updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recipe with ID: {RecipeId}", id);
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Failed to update recipe",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete a recipe
        /// </summary>
        /// <param name="id">Recipe ID</param>
        /// <returns>Deletion confirmation</returns>
        [HttpDelete("recipes/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public ActionResult<ApiResponse<object>> DeleteRecipe(int id)
        {
            try
            {
                _logger.LogInformation("Deleting recipe with ID: {RecipeId}", id);

                var recipe = _recipeService.Get(id);
                if (recipe == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Message = $"Recipe with ID {id} not found"
                    });
                }

                _recipeService.Delete(id);

                return Ok(new ApiResponse<object>
                {
                    Data = new { Id = id },
                    Success = true,
                    Message = "Recipe deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recipe with ID: {RecipeId}", id);
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Failed to delete recipe",
                    Details = ex.Message
                });
            }
        }

        #endregion

        #region Category Endpoints

        /// <summary>
        /// Get all recipe categories
        /// </summary>
        /// <param name="includeInactive">Include inactive categories</param>
        /// <returns>List of categories</returns>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RecipeCategoryModel>>), StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<IEnumerable<RecipeCategoryModel>>> GetCategories(
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting categories, includeInactive: {IncludeInactive}", includeInactive);

                var categories = _recipeService.GetRecipeCategoryList()
                    .Where(c => includeInactive || c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToList();

                return Ok(new ApiResponse<IEnumerable<RecipeCategoryModel>>
                {
                    Data = categories,
                    Success = true,
                    Message = $"Retrieved {categories.Count} categories"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Failed to retrieve categories",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get a specific category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        [HttpGet("categories/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<RecipeCategoryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public ActionResult<ApiResponse<RecipeCategoryModel>> GetCategory(int id)
        {
            try
            {
                _logger.LogInformation("Getting category with ID: {CategoryId}", id);

                var category = _recipeService.GetRecipeCategoryById(id);
                if (category == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Message = $"Category with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<RecipeCategoryModel>
                {
                    Data = category,
                    Success = true,
                    Message = "Category retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID: {CategoryId}", id);
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Failed to retrieve category",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="request">Category creation request</param>
        /// <returns>Created category</returns>
        [HttpPost("categories")]
        [ProducesResponseType(typeof(ApiResponse<RecipeCategoryModel>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult<ApiResponse<RecipeCategoryModel>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid category data",
                        ValidationErrors = ModelState.GetValidationErrors()
                    });
                }

                _logger.LogInformation("Creating new category: {CategoryName}", request.Name);

                var category = new RecipeCategoryModel
                {
                    Name = request.Name,
                    Description = request.Description ?? string.Empty,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = request.IsActive,
                    Url = !string.IsNullOrEmpty(request.Url)
                        ? request.Url.ToLower().Replace(" ", "-").Replace("_", "-").Replace(".", "-")
                        : request.Name.ToLower().Replace(" ", "-").Replace("_", "-").Replace(".", "-")
                };

                var savedCategory = _recipeService.Save(category);

                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = savedCategory.Id },
                    new ApiResponse<RecipeCategoryModel>
                    {
                        Data = savedCategory,
                        Success = true,
                        Message = "Category created successfully"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Failed to create category",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="request">Category update request</param>
        /// <returns>Updated category</returns>
        [HttpPut("categories/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<RecipeCategoryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult<ApiResponse<RecipeCategoryModel>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid category data",
                        ValidationErrors = ModelState.GetValidationErrors()
                    });
                }

                _logger.LogInformation("Updating category with ID: {CategoryId}", id);

                var existingCategory = _recipeService.GetRecipeCategoryById(id);
                if (existingCategory == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Message = $"Category with ID {id} not found"
                    });
                }

                // Update properties
                existingCategory.Name = request.Name;
                existingCategory.Description = request.Description ?? string.Empty;
                existingCategory.DisplayOrder = request.DisplayOrder;
                existingCategory.IsActive = request.IsActive;
                existingCategory.Url = !string.IsNullOrEmpty(request.Url)
                    ? request.Url.ToLower().Replace(" ", "-").Replace("_", "-").Replace(".", "-")
                    : existingCategory.Url;

                var updatedCategory = _recipeService.Save(existingCategory);

                return Ok(new ApiResponse<RecipeCategoryModel>
                {
                    Data = updatedCategory,
                    Success = true,
                    Message = "Category updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID: {CategoryId}", id);
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Failed to update category",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Deletion confirmation</returns>
        [HttpDelete("categories/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public ActionResult<ApiResponse<object>> DeleteCategory(int id)
        {
            try
            {
                _logger.LogInformation("Deleting category with ID: {CategoryId}", id);

                var category = _recipeService.GetRecipeCategoryById(id);
                if (category == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Message = $"Category with ID {id} not found"
                    });
                }

                _recipeService.Delete(category);

                return Ok(new ApiResponse<object>
                {
                    Data = new { Id = id },
                    Success = true,
                    Message = "Category deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID: {CategoryId}", id);
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Failed to delete category",
                    Details = ex.Message
                });
            }
        }

        #endregion
    }

    #region Request/Response Models

    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PaginationInfo? Pagination { get; set; }
    }

    /// <summary>
    /// API error response
    /// </summary>
    public class ApiErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
    }

    /// <summary>
    /// Pagination information
    /// </summary>
    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }

    /// <summary>
    /// Request model for creating a recipe
    /// </summary>
    public class CreateRecipeRequest
    {
        [Required(ErrorMessage = "Recipe name is required")]
        [StringLength(255, ErrorMessage = "Recipe name cannot exceed 255 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Ingredients { get; set; }

        public string? Instructions { get; set; }

        [Range(1, 999, ErrorMessage = "Servings must be between 1 and 999")]
        public int Servings { get; set; } = 1;

        public string? AuthorName { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
    }

    /// <summary>
    /// Request model for updating a recipe
    /// </summary>
    public class UpdateRecipeRequest
    {
        [Required(ErrorMessage = "Recipe name is required")]
        [StringLength(255, ErrorMessage = "Recipe name cannot exceed 255 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Ingredients { get; set; }

        public string? Instructions { get; set; }

        [Range(1, 999, ErrorMessage = "Servings must be between 1 and 999")]
        public int Servings { get; set; } = 1;

        public string? AuthorName { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
    }

    /// <summary>
    /// Request model for creating a category
    /// </summary>
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(1, 999, ErrorMessage = "Display order must be between 1 and 999")]
        public int DisplayOrder { get; set; } = 99;

        public bool IsActive { get; set; } = true;

        [StringLength(100, ErrorMessage = "URL cannot exceed 100 characters")]
        public string? Url { get; set; }
    }

    /// <summary>
    /// Request model for updating a category
    /// </summary>
    public class UpdateCategoryRequest
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(1, 999, ErrorMessage = "Display order must be between 1 and 999")]
        public int DisplayOrder { get; set; } = 99;

        public bool IsActive { get; set; } = true;

        [StringLength(100, ErrorMessage = "URL cannot exceed 100 characters")]
        public string? Url { get; set; }
    }

    #endregion

    #region Extension Methods

    /// <summary>
    /// Extension methods for ModelState validation
    /// </summary>
    public static class ModelStateExtensions
    {
        public static Dictionary<string, string[]> GetValidationErrors(this Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            return modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
        }
    }

    #endregion
}