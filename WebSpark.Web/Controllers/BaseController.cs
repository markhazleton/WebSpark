using System.Text.Json;
using WebSpark.Bootswatch.Provider;
using WebSpark.HttpClientUtility.RequestResult;
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
                using (var scope = HttpContext.RequestServices.CreateScope())
                {
                    var _websiteService = scope.ServiceProvider.GetRequiredService<Core.Interfaces.IWebsiteService>();
                    _baseView = _websiteService.GetBaseViewByHostAsync(HttpContext.Request.Host.Host, _DefaultSiteId).Result;
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<BootswatchStyleProvider>>();
                    var service = scope.ServiceProvider.GetRequiredService<IHttpRequestResultService>();
                    var styleService = new BootswatchStyleProvider(logger, service);
                    var bootswatchModels = styleService.GetAsync().Result;
                    _baseView.StyleList = Create(bootswatchModels);
                }
                var RequestScheme = HttpContext.Request.Scheme; // Changed to use actual request scheme

                var curSiteRoot = ($"{RequestScheme}://{HttpContext.Request.Host.Host}:{HttpContext.Request.Host.Port}/");
                _baseView.SiteUrl = new Uri(curSiteRoot);
                HttpContext.Session.SetString(SessionExtensionsKeys.BaseViewKey, JsonSerializer.Serialize(_baseView, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
                _logger.LogDebug("Loaded BaseView From Database");
            }
            return _baseView;
        }
    }
    private IEnumerable<Core.Models.StyleModel> Create(IEnumerable<Bootswatch.Model.StyleModel> bootswatchModels)
    {
        foreach (var item in bootswatchModels)
        {
            yield return new Core.Models.StyleModel
            {
                name = item.name,
                cssCdn = item.cssCdn,
                css = item.css,
                scss = item.scss,
                scssVariables = item.scssVariables,
                cssMin = item.cssMin,
                description = item.description,
                less = item.less,
                lessVariables = item.lessVariables,
                preview = item.preview,
                thumbnail = item.thumbnail
            };
        }
    }
}
