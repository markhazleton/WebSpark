using System.Net;
using WebSpark.Core.Infrastructure.Middleware;
using WebSpark.Domain.Interfaces;
using WebSpark.RecipeManager.Interfaces;
using WebSpark.RecipeManager.Models;

namespace WebSpark.WebMvc.Controllers.Api.Cms;

/// <summary>
/// Recipe Category
/// </summary>
/// <remarks>
/// Recipe Controller constructor
/// </remarks>
/// <param name="logger"></param>
/// <param name="scopeInfo"></param>
/// <param name="dbRecipe"></param>
[Route("api/Recipe/Category/")]
public class RecipeCategoryController(ILogger<RecipeCategoryController> logger, IScopeInformation scopeInfo, IRecipeService dbRecipe) : ApiBaseController
{
    private readonly ILogger<RecipeCategoryController> _logger = logger;
    private readonly IScopeInformation _scopeInfo = scopeInfo;
    private readonly IRecipeService _recipeService = dbRecipe;

    /// <summary>
    /// Get All Recipe Categories
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IEnumerable<RecipeCategoryModel> Get()
    {
        var myList = _recipeService.GetRecipeCategoryList();
        foreach (var r in myList)
        {
            r.Url = this.Url.ActionLink(action: null, controller: "RecipeCategory", values: new { id = r.Id });
        }
        return myList;
    }

    /// <summary>
    /// Get Recipe by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public RecipeCategoryModel Get(int id)
    {
        var rc = _recipeService.GetRecipeCategoryById(id);
        if (rc.Id == 0) throw new StatusCodeException(HttpStatusCode.NotFound); ;

        rc.Url = GetObjectUrl(rc.Id);
        foreach (var r in rc.Recipes)
        {
            r.RecipeURL = GetObjectUrl(r.Id, "Recipe");
            r.RecipeCategory.Url = this.Url.ActionLink(action: null, controller: "RecipeCategory", values: new { id = r.RecipeCategory.Id });
        }
        return rc;

    }
    /// <summary>
    /// Add New Recipe Category
    /// </summary>
    /// <param name="value"></param>
    [HttpPost]
    public void Post([FromBody] RecipeCategoryModel value)
    {
    }

    /// <summary>
    /// Update Existing Recipe Category
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] RecipeCategoryModel value)
    {
    }

    /// <summary>
    /// Delete Recipe Category by Id
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
