using System.Text.Json;
using WebSpark.Bootswatch.Provider;
using WebSpark.Domain.Interfaces;
using WebSpark.Domain.ViewModels;
using WebSpark.Web.Extensions;

namespace WebSpark.Web.Controllers;
/// <summary>
/// Base Controller for all Controllers
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="logger"></param>
/// <param name="configuration"></param>
/// <param name="websiteService"></param>
public class BaseController(ILogger logger, IConfiguration configuration, IWebsiteService websiteService) : Controller
{
    protected readonly ILogger _logger = logger;
    protected readonly IWebsiteService _websiteService = websiteService;
    protected readonly IConfiguration _config = configuration;

    /// <summary>
    /// Is Cache Enabled
    /// </summary>
    /// <returns></returns>
    protected bool IsCacheEnabled()
    {
        return _config.GetSection("ControlSpark").GetValue<bool>("CacheEnabled");
    }
    /// <summary>
    /// Base View for Page Rendering
    /// </summary>
    private WebsiteVM? _baseView = null;

    /// <summary>
    /// 
    /// </summary>
    protected WebsiteVM BaseVM
    {
        get
        {
            if (IsCacheEnabled())
            {
                _baseView = HttpContext.Session.Get<WebsiteVM>(SessionExtensionsKeys.BaseViewKey);
                _logger.LogInformation("Loaded BaseView From Session");
            }

            if (_baseView == null)
            {
                var _DefaultSiteId = _config.GetSection("ControlSpark").GetValue<string>("DefaultSiteId");
                
                _baseView = _websiteService.GetBaseViewByHostAsync(HttpContext.Request.Host.Host, _DefaultSiteId).GetAwaiter().GetResult();

                var styleService = new BootswatchStyleProvider();
                _baseView.StyleList = styleService.Get();

                var RequestScheme = "https"; // HttpContext.Request.Scheme;

                var curSiteRoot = ($"{RequestScheme}://{HttpContext.Request.Host.Host}:{HttpContext.Request.Host.Port}/");
                _baseView.SiteUrl = new Uri(curSiteRoot);
                HttpContext.Session.SetString(SessionExtensionsKeys.BaseViewKey, JsonSerializer.Serialize(_baseView, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
                _logger.LogInformation("Loaded BaseView From Database");
            }
            return _baseView;
        }
    }
}
