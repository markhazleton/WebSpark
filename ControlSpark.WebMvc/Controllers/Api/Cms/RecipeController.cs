using ControlSpark.RecipeManager.Interfaces;
using ControlSpark.RecipeManager.Models;

namespace ControlSpark.WebMvc.Controllers.Api.Cms;

/// <summary>
/// Recipe Controller
/// </summary>
public class RecipeController : ApiBaseController
{
    private readonly ILogger<RecipeController> _logger;
    private readonly IScopeInformation _scopeInfo;
    private readonly IRecipeService _dbRecipe;
    /// <summary>
    /// Recipe Controller constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="scopeInfo"></param>
    /// <param name="dbRecipe"></param>
    public RecipeController(ILogger<RecipeController> logger, IScopeInformation scopeInfo, IRecipeService dbRecipe)
    {
        _dbRecipe = dbRecipe;
        _logger = logger;
        _scopeInfo = scopeInfo;
    }

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
