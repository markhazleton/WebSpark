using System.Text.Json;
using WebSpark.Bootswatch.Model;
using WebSpark.Bootswatch.Provider;
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
public class BaseController(ILogger logger, IConfiguration configuration, Core.Interfaces.IWebsiteService websiteService) : Controller
{
    protected readonly ILogger _logger = logger;
    protected readonly Core.Interfaces.IWebsiteService _websiteService = websiteService;
    protected readonly IConfiguration _config = configuration;
    private WebsiteVM? _baseView = null;
    protected readonly JsonSerializerOptions optionsJsonSerializer = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Is Cache Enabled
    /// </summary>
    /// <returns></returns>
    protected bool IsCacheEnabled()
    {
        return _config.GetSection("WebSpark").GetValue<bool>("CacheEnabled");
    }
    /// <summary>
    /// Base View for Page Rendering
    /// </summary>
    protected string SetCurrentStyle(string currentStyle)
    {
        _baseView ??= BaseVM;
        _baseView.CurrentStyle = currentStyle;
        HttpContext.Session.SetString(
            SessionExtensionsKeys.CurrentViewKey,
            JsonSerializer.Serialize(_baseView, optionsJsonSerializer));

        return _baseView.CurrentStyle;
    }
    /// <summary>
    /// 
    /// </summary>
    protected WebsiteVM BaseVM
    {
        get
        {
            // check for current view first
            var curView = HttpContext.Session.Get<WebsiteVM>(SessionExtensionsKeys.CurrentViewKey);
            if (curView != null)
            {
                _baseView = curView;
                _logger.LogDebug("Loaded BaseView From Session:CurrentViewKey");
                return curView;
            }

            if (IsCacheEnabled())
            {
                _baseView = HttpContext.Session.Get<WebsiteVM>(SessionExtensionsKeys.BaseViewKey);
                _logger.LogDebug("Loaded BaseView From Session");
            }

            if (_baseView == null)
            {
                var _DefaultSiteId = _config.GetSection("WebSpark").GetValue<string>("DefaultSiteId");

                _baseView = _websiteService.GetBaseViewByHostAsync(HttpContext.Request.Host.Host, _DefaultSiteId).GetAwaiter().GetResult();

                var styleService = new BootswatchStyleProvider();
                _baseView.StyleList = Create(styleService.Get());

                var RequestScheme = "https"; // HttpContext.Request.Scheme;

                var curSiteRoot = ($"{RequestScheme}://{HttpContext.Request.Host.Host}:{HttpContext.Request.Host.Port}/");
                _baseView.SiteUrl = new Uri(curSiteRoot);
                HttpContext.Session.SetString(SessionExtensionsKeys.BaseViewKey, JsonSerializer.Serialize(_baseView, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
                _logger.LogDebug("Loaded BaseView From Database");
            }
            return _baseView;
        }
    }

    private static IEnumerable<StyleModel> Create(IEnumerable<BootswatchStyleModel> enumerable)
    {
        var list = new List<StyleModel>();
        foreach (var item in enumerable)
        {
            list.Add(new StyleModel
            {
                name = item.name,
                css = item.css,
                description = item.description,
                preview = item.preview,
                scss = item.scss,
                scssVariables = item.scssVariables,
                cssCdn = item.cssCdn,
                cssMin = item.cssMin,
                less = item.less,
                lessVariables = item.lessVariables,
                thumbnail = item.thumbnail
            });
        }
        return list;
    }
}
