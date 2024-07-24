using System.Text.Json;
using WebSpark.Core.Infrastructure;

namespace WebSpark.Web.Controllers;

/// <summary>
/// 
/// </summary>
public class RecipeController(
    ILogger<RecipeController> logger,
    IConfiguration config,
    Core.Interfaces.IWebsiteService websiteService,
    Core.Interfaces.IRecipeService recipeProvider) : BaseController(logger, config, websiteService)
{
    private RecipeVM? _viewModel;
    private const string RecipeViewKey = "RecipeViewKey";
    private RecipeVM RecipeViewModel
    {
        get
        {
            if (IsCacheEnabled())
            {
                _logger.LogDebug("Loaded RecipeView From Session");
            }
            if (_viewModel == null)
            {
                var _DefaultSiteId = _config.GetValue<string>("DefaultSiteId");
                var x = HttpContext.Request.Host;
                var task = Task.Run(() => recipeProvider.GetRecipeVMHostAsync(HttpContext.Request.Host.Host, BaseVM));
                _viewModel = task.GetAwaiter().GetResult();
                _viewModel.CurrentStyle = BaseVM.CurrentStyle;
                HttpContext.Session.SetString(RecipeViewKey, JsonSerializer.Serialize(_viewModel, optionsJsonSerializer));
                _logger.LogDebug("Loaded RecipeView From Database");
            }
            return _viewModel;
        }
    }

    /// <summary>
    /// Category
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public ActionResult Category(string? id = null)
    {
        if (!string.IsNullOrEmpty(id))
        {
            RecipeViewModel.Category = RecipeViewModel.CategoryList.Where(w => FormatHelper.GetSafePath(w.Name) == FormatHelper.GetSafePath(id)).FirstOrDefault();
            if (RecipeViewModel.Category != null)
            {
                if (string.Compare(HttpContext.Request.Path, RecipeViewModel.Category.Url, StringComparison.Ordinal) != 0)
                {
                    return View($"~/Views/Templates/{BaseVM.Template}/Recipe/Category.cshtml", RecipeViewModel);
                }
                RecipeViewModel.PageTitle = $"{RecipeViewModel.Category.Name}";
                RecipeViewModel.MetaDescription = $"{RecipeViewModel.Category.Description}";
                RecipeViewModel.MetaKeywords = $"{RecipeViewModel.Category.Name}";
            }
        }
        return View($"~/Views/Templates/{BaseVM.Template}/Recipe/Category.cshtml", RecipeViewModel);
    }

    /// <summary>
    /// Get Page
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>ActionResult.</returns>
    [HttpGet]
    public ActionResult Index(string? id = null)
    {
        if (string.IsNullOrEmpty(id))
        {
            return View($"~/Views/Templates/{BaseVM.Template}/Recipe/Category.cshtml", RecipeViewModel);
        }
        else
        {
            RecipeViewModel.Recipe = RecipeViewModel.RecipeList
                .Where(w => FormatHelper.GetSafePath(w.Name) == FormatHelper.GetSafePath(id)).FirstOrDefault();

            if (RecipeViewModel.Recipe == null)
                return Redirect($"/recipe");

            if (string.Compare(HttpContext.Request.Path, RecipeViewModel.Recipe.RecipeURL, StringComparison.Ordinal) != 0)
            {
                return Redirect($"/{RecipeViewModel.Recipe.RecipeURL}");
            }
            RecipeViewModel.PageTitle = $"{RecipeViewModel.Recipe.Name}";
            RecipeViewModel.MetaDescription = $"{RecipeViewModel.Recipe.Description}";
            RecipeViewModel.MetaKeywords = $"{RecipeViewModel.Recipe.Name}";
        }
        return View($"~/Views/Templates/{BaseVM.Template}/Recipe/Index.cshtml", RecipeViewModel);
    }
}
