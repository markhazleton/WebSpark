namespace ControlSpark.Web.Controllers;

[Route("error/")]
public class ErrorController : BaseController
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    public ErrorController(ILogger<ErrorController> logger,
        IConfiguration config,
        IWebsiteService websiteService)
        : base(logger, config, websiteService)
    {

    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [Route("notfound/{slug}")]
    public IActionResult NotFound(string slug = null)
    {
        _logger.LogWarning("Path Not Found");
        var errorModel = new ErrorVM(BaseVM, slug);

        var myPage = BaseVM?.Menu?.Where(w => w.Url.IsMatch(slug)).FirstOrDefault();
        if (myPage is not null) return Redirect(myPage.Url);


        return View(errorModel);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [Route("{slug}")]
    public IActionResult Index(string slug = null)
    {
        _logger.LogWarning("Default Error Page");
        var errorModel = new ErrorVM(BaseVM, slug);
        return View(errorModel);
    }
}
