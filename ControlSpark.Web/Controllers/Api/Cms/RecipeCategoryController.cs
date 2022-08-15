using ControlSpark.Core.Infrastructure.Middleware;
using ControlSpark.RecipeManager.Interfaces;
using ControlSpark.RecipeManager.Models;
using System.Net;

namespace ControlSpark.Web.Controllers.Api.Cms;

/// <summary>
/// Recipe Category
/// </summary>
[Route("api/Recipe/Category/")]
public class RecipeCategoryController : ApiBaseController
{
    private readonly ILogger<RecipeCategoryController> _logger;
    private readonly IScopeInformation _scopeInfo;
    private readonly IRecipeService _recipeService;
    /// <summary>
    /// Recipe Controller constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="scopeInfo"></param>
    /// <param name="dbRecipe"></param>
    public RecipeCategoryController(ILogger<RecipeCategoryController> logger, IScopeInformation scopeInfo, IRecipeService dbRecipe)
    {
        _recipeService = dbRecipe;
        _logger = logger;
        _scopeInfo = scopeInfo;
    }

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
