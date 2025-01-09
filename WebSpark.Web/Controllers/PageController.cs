namespace WebSpark.Web.Controllers;

/// <summary>
/// CMS Page Controller
/// </summary>
/// <remarks>
/// PageController Constructor
/// </remarks>
/// <param name="logger"></param>
/// <param name="config"></param>
/// <param name="websiteService"></param>
public class PageController(
    ILogger<PageController> logger,
    IConfiguration config,
    Core.Interfaces.IWebsiteService websiteService) :
    BaseController(
        logger,
        config,
        websiteService)
{
    /// <summary>
    /// Get Page
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>ActionResult.</returns>
    [HttpGet]
    public ActionResult Index(string id)
    {

        PageVM pageVM = new(BaseVM)
        {
            Item = BaseVM.Menu.Where(w => w.Argument == id?.ToLower(CultureInfo.CurrentCulture) && w.Controller == "Page").FirstOrDefault()
        };
        if (pageVM.Item == null || id == null)
        {
            pageVM.Item = BaseVM.Menu?.Where(w => w.ParentId == null).OrderBy(o => o.DisplayOrder).FirstOrDefault();

            // return Redirect($"{page?.Url}");
        }
        // TODO: Find way to get absolute path to check for auto redirect
        //if (string.Compare(httpContext.Request.Url.AbsolutePath, ($"/{page.Url}"), StringComparison.Ordinal) != 0)
        //{
        //    return Redirect($"/{page.Url}");
        //}

        pageVM.PageTitle = pageVM.Item?.Title ?? "Page Not Found";
        pageVM.MetaDescription = pageVM.Item?.Description ?? "Page Not Found";
        pageVM.MetaKeywords = pageVM.Item?.Action ?? "Page Not Found";
        return View($"~/Views/Templates/{pageVM.Template}/Page/Default.cshtml", pageVM);
    }
}
