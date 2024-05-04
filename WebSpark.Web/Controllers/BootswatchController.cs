using System.Text.Json;
using WebSpark.Domain.Interfaces;
using WebSpark.Domain.ViewModels;
using WebSpark.Web.Extensions;

namespace WebSpark.Web.Controllers;

/// <summary>
/// Pick Style for WebSite
/// </summary>
public class BootswatchController : BaseController
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    public BootswatchController(ILogger<BootswatchController> logger, IConfiguration config, IWebsiteService websiteService)
        : base(logger, config, websiteService)
    {

    }
    /// <summary>
    /// Get Page
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>ActionResult.</returns>
    [HttpGet]
    public ActionResult Index(string id = null)
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
    public ActionResult Index(WebsiteVM postWebsiteVM, string id = null)
    {
        BaseVM.WebsiteStyle = postWebsiteVM.WebsiteStyle ?? BaseVM.WebsiteStyle;
        HttpContext.Session.SetString(SessionExtensionsKeys.BaseViewKey, JsonSerializer.Serialize(BaseVM, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
        return View($"~/Views/Templates/{BaseVM.Template}/Bootswatch/Index.cshtml", BaseVM);
    }
}
