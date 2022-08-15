namespace ControlSpark.Web.Controllers;

/// <summary>
/// CMS Page Controller
/// </summary>
public class PageController : BaseController
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    public PageController(ILogger<PageController> logger, IConfiguration config, IWebsiteService websiteService)
        : base(logger, config, websiteService)
    {
    }
    /// <summary>
    /// Get Page
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>ActionResult.</returns>
    [HttpGet]
    public ActionResult Index(string id)
    {
        var page = BaseVM.Menu.Where(w => w.Argument == id?.ToLower(CultureInfo.CurrentCulture) && w.Controller == "Page").FirstOrDefault();
        if (page == null || id == null)
        {
            page = BaseVM.Menu?.Where(w => w.ParentId == null).OrderBy(o => o.DisplayOrder).FirstOrDefault();
            return Redirect($"{page?.Url}");
        }
        // TODO: Find way to get absolute path to check for auto redirect
        //if (string.Compare(httpContext.Request.Url.AbsolutePath, ($"/{page.Url}"), StringComparison.Ordinal) != 0)
        //{
        //    return Redirect($"/{page.Url}");
        //}
        BaseVM.PageTitle = page?.Title ?? "Page Not Found";
        BaseVM.MetaDescription = page?.Description ?? "Page Not Found";
        BaseVM.MetaKeywords = page?.Action ?? "Page Not Found";

        return View($"~/Views/Templates/{BaseVM.Template}/Page/Default.cshtml", BaseVM);
    }
}
