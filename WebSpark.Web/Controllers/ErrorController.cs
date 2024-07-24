using WebSpark.Core.Extensions;
using WebSpark.Core.Models.ViewModels;

namespace WebSpark.Web.Controllers;

/// <summary>
/// 
/// </summary>
/// <param name="configuration"></param>
[Route("error/")]
public class ErrorController(ILogger<ErrorController> logger,
    IConfiguration config,
    Core.Interfaces.IWebsiteService websiteService) : BaseController(logger, config, websiteService)
{
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

        return View($"~/Views/Templates/{BaseVM.Template}/error/NotFound.cshtml", errorModel);

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
        return View($"~/Views/Templates/{BaseVM.Template}/error/Index.cshtml", errorModel);
    }
}
