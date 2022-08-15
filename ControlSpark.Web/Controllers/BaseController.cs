using ControlSpark.Bootswatch.Provider;
using ControlSpark.Web.Extensions;
using System.Text.Json;

namespace ControlSpark.Web.Controllers;
public class BaseController : Controller
{
    public const string BaseViewKey = "BaseViewKey";

    protected readonly ILogger _logger;
    protected readonly IWebsiteService _websiteService;
    protected readonly IConfiguration _config;
    public BaseController(ILogger logger, IConfiguration configuration, IWebsiteService websiteService)
    {
        _websiteService = websiteService;
        _config = configuration;
        _logger = logger;
        _baseView = null;
    }

    /// <summary>
    /// Base View for Page Rendering
    /// </summary>
    private WebsiteVM? _baseView;

    /// <summary>
    /// 
    /// </summary>
    protected WebsiteVM BaseVM
    {
        get
        {
            if (_baseView == null)
            {
                _baseView = HttpContext.Session.Get<WebsiteVM>(SessionExtensionsKeys.BaseViewKey);
                _logger.LogInformation("Loaded BaseView From Session");
            }
            if (_baseView == null)
            {
                var _DefaultSiteId = _config.GetValue<string>("DefaultSiteId");
                var x = HttpContext.Request.Host;
                var task = Task.Run(() => _websiteService.GetBaseViewByHostAsync(HttpContext.Request.Host.Host, _DefaultSiteId));
                _baseView = task.GetAwaiter().GetResult();
                var styleService = new BootswatchStyleProvider();
                _baseView.StyleList = styleService.Get();
                var curSiteRoot = ($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Host}:{HttpContext.Request.Host.Port}/");
                _baseView.SiteUrl = new Uri(curSiteRoot);
                HttpContext.Session.SetString(BaseViewKey, JsonSerializer.Serialize(_baseView, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
                _logger.LogInformation("Loaded BaseView From Database");
            }
            return _baseView;
        }
    }
}
