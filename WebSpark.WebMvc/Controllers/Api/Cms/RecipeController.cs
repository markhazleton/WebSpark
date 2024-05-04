using WebSpark.Domain.Interfaces;
using WebSpark.RecipeManager.Interfaces;
using WebSpark.RecipeManager.Models;

namespace WebSpark.WebMvc.Controllers.Api.Cms;

/// <summary>
/// Recipe Controller
/// </summary>
/// <remarks>
/// Recipe Controller constructor
/// </remarks>
/// <param name="logger"></param>
/// <param name="scopeInfo"></param>
/// <param name="dbRecipe"></param>
public class RecipeController(ILogger<RecipeController> logger, IScopeInformation scopeInfo, IRecipeService dbRecipe) : ApiBaseController
{
    private readonly ILogger<RecipeController> _logger = logger;
    private readonly IScopeInformation _scopeInfo = scopeInfo;
    private readonly IRecipeService _dbRecipe = dbRecipe;

    /// <summary>
    /// Get All Recipes
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IEnumerable<RecipeModel> Get()
    {
        var myList = _dbRecipe.Get();
        foreach (var r in myList)
        {
            r.RecipeURL = GetObjectUrl(r.Id);
            r.RecipeCategory.Url = GetObjectUrl(r.RecipeCategory.Id, "RecipeCategory");
        }
        return myList;
    }

    /// <summary>
    /// Get Recipe by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public RecipeModel Get(int id)
    {
        var r = _dbRecipe.Get(id);
        r.RecipeURL = GetObjectUrl(r.Id);
        r.RecipeCategory.Url = GetObjectUrl(r.RecipeCategory.Id, "RecipeCategory");
        return r;
    }

    /// <summary>
    /// Add New Recipe
    /// </summary>
    /// <param name="value"></param>
    [HttpPost]
    public void Post([FromBody] RecipeModel value)
    {
    }

    /// <summary>
    /// Update Existing Recipe
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] RecipeModel value)
    {
    }

    /// <summary>
    /// Delete Recipe by Id
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
