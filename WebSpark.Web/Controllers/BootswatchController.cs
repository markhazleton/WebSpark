using System.Text.Json;
using WebSpark.Core.Models.ViewModels;
using WebSpark.Web.Extensions;

namespace WebSpark.Web.Controllers;

/// <summary>
/// Pick Style for WebSite
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="configuration"></param>
public class BootswatchController(
    ILogger<BootswatchController> logger,
    IConfiguration config,
    Core.Interfaces.IWebsiteService websiteService) : BaseController(logger, config, websiteService)
{

    /// <summary>
    /// Get Page
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>ActionResult.</returns>
    [HttpGet]
    public ActionResult Index(string? id = null)
    {
        BaseVM.PageTitle = "Style";
        BaseVM.MetaDescription = "Style";
        BaseVM.MetaKeywords = "Style";
        return View($"~/Views/Templates/{BaseVM.Template}/Bootswatch/Index.cshtml", BaseVM);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="postWebsiteVM"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult Index(WebsiteVM postWebsiteVM, string? id = null)
    {
        SetCurrentStyle(postWebsiteVM.WebsiteStyle ?? BaseVM.WebsiteStyle);
        BaseVM.CurrentStyle = postWebsiteVM.WebsiteStyle ?? BaseVM.WebsiteStyle;
        HttpContext.Session.SetString(SessionExtensionsKeys.CurrentViewKey, JsonSerializer.Serialize(BaseVM, optionsJsonSerializer));
        return View($"~/Views/Templates/{BaseVM.Template}/Bootswatch/Index.cshtml", BaseVM);
    }
}
