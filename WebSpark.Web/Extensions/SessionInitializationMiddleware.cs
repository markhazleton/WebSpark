using System.Text.Json;
using WebSpark.Bootswatch.Provider;

namespace WebSpark.Web.Extensions;

/// <summary>
/// Middleware for initializing session variables.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SessionInitializationMiddleware"/> class.
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
public class SessionInitializationMiddleware(
    RequestDelegate _next,
    IConfiguration _config)
{

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Session.GetString("Initialized") == null)
        {
            var _DefaultSiteId = _config.GetSection("ControlSpark").GetValue<string>("DefaultSiteId");

            using (var scope = context.RequestServices.CreateScope())
            {
                var _websiteService = scope.ServiceProvider.GetRequiredService<Core.Interfaces.IWebsiteService>();
                var _baseView = await _websiteService.GetBaseViewByHostAsync(context.Request.Host.Host, _DefaultSiteId);

                var styleService = new BootswatchStyleProvider();
                _baseView.StyleList = Create(styleService.Get());

                var RequestScheme = "https";
                var curSiteRoot = $"{RequestScheme}://{context.Request.Host.Host}:{context.Request.Host.Port}/";
                _baseView.SiteUrl = new Uri(curSiteRoot);

                context.Session.SetString(
                    SessionExtensionsKeys.BaseViewKey,
                    JsonSerializer.Serialize(_baseView, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));

                context.Session.SetString("DefaultSiteId", _DefaultSiteId ?? "1");
                context.Session.SetString("Initialized", "true");
            }
        }
        await _next(context);
    }

    private static IEnumerable<Core.Models.StyleModel> Create(IEnumerable<Bootswatch.Model.BootswatchStyleModel> enumerable)
    {
        var list = new List<Core.Models.StyleModel>();
        foreach (var item in enumerable)
        {
            list.Add(new Core.Models.StyleModel
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
            });
        }
        return list;
    }
}

/// <summary>
/// Extension method used to add the <see cref="SessionInitializationMiddleware"/> to the HTTP request pipeline.
/// </summary>
public static class SessionInitializationMiddlewareExtensions
{
    /// <summary>
    /// Adds the <see cref="SessionInitializationMiddleware"/> to the HTTP request pipeline.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
    public static IApplicationBuilder UseSessionInitialization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionInitializationMiddleware>();
    }
}
