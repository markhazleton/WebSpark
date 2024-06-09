using System.Text.Json;
using WebSpark.Core.Infrastructure;
using WebSpark.Domain.Interfaces;
using WebSpark.RecipeManager.Interfaces;
using WebSpark.RecipeManager.ViewModels;

namespace WebSpark.Web.Controllers;

/// <summary>
/// 
/// </summary>
public class RecipeController(
    ILogger<RecipeController> logger,
    IConfiguration config,
    IWebsiteService websiteService,
    IRecipeService recipeProvider) : BaseController(logger, config, websiteService)
{
    private RecipeVM? _viewModel;
    private const string RecipeViewKey = "RecipeViewKey";

    private RecipeVM viewModel
    {
        get
        {
            if (IsCacheEnabled())
            {
                //    _viewModel = HttpContext.Session.Get<RecipeVM>(RecipeViewKey);
                _logger.LogInformation("Loaded RecipeView From Session");
            }

            if (_viewModel == null)
            {
                var _DefaultSiteId = _config.GetValue<string>("DefaultSiteId");
                var x = HttpContext.Request.Host;
                var task = Task.Run(() => recipeProvider.GetRecipeVMHostAsync(HttpContext.Request.Host.Host, BaseVM));
                _viewModel = task.GetAwaiter().GetResult();

                HttpContext.Session.SetString(RecipeViewKey, JsonSerializer.Serialize(_viewModel, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
                _logger.LogInformation("Loaded RecipeView From Database");
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
    public ActionResult Category(string id = null)
    {
        if (!string.IsNullOrEmpty(id))
        {
            viewModel.Category = viewModel.CategoryList.Where(w => FormatHelper.GetSafePath(w.Name) == FormatHelper.GetSafePath(id)).FirstOrDefault();

            if (string.Compare(HttpContext.Request.Path, viewModel.Category.Url, StringComparison.Ordinal) != 0)
            {
                return Redirect($"/{viewModel.Category.Url}");
            }
            viewModel.PageTitle = $"{viewModel.Category.Name}";
            viewModel.MetaDescription = $"{viewModel.Category.Description}";
            viewModel.MetaKeywords = $"{viewModel.Category.Name}";
        }
        return View($"~/Views/Templates/{BaseVM.Template}/Recipe/Category.cshtml", viewModel);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="recipeVM"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult Category(RecipeVM recipeVM, string id = null)
    {
        viewModel.Category = viewModel
            .CategoryList.Where(w => FormatHelper.GetSafePath(w.Name) == FormatHelper.GetSafePath(recipeVM.Category?.Name)).FirstOrDefault();
        if (viewModel.Category == null)
            return Redirect($"/recipe/category");

        return Redirect(viewModel.Category.Url);
    }
    /// <summary>
    /// Get Page
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>ActionResult.</returns>
    [HttpGet]
    public ActionResult Index(string id = null)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Redirect($"/recipe/category");
        }
        else
        {
            viewModel.Recipe = viewModel.RecipeList
                .Where(w => FormatHelper.GetSafePath(w.Name) == FormatHelper.GetSafePath(id)).FirstOrDefault();

            if (viewModel.Recipe == null)
                return Redirect($"/recipe");

            if (string.Compare(HttpContext.Request.Path, viewModel.Recipe.RecipeURL, StringComparison.Ordinal) != 0)
            {
                return Redirect($"/{viewModel.Recipe.RecipeURL}");
            }
            viewModel.PageTitle = $"{viewModel.Recipe.Name}";
            viewModel.MetaDescription = $"{viewModel.Recipe.Description}";
            viewModel.MetaKeywords = $"{viewModel.Recipe.Name}";
        }
        return View($"~/Views/Templates/{BaseVM.Template}/Recipe/Index.cshtml", viewModel);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="recipeVM"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult Index(RecipeVM recipeVM, string id = null)
    {
        viewModel.Recipe = viewModel?.RecipeList
            .Where(w => FormatHelper.GetSafePath(w.Name) == FormatHelper.GetSafePath(recipeVM.Recipe?.Name))
            .FirstOrDefault();
        if (viewModel?.Recipe == null)
            return Redirect($"/recipe");

        return Redirect(viewModel.Recipe.RecipeURL);
    }
}
