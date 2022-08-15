using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json;

namespace ControlSpark.Web.Extensions;

/// <summary>
/// 
/// </summary>
public class WebRouteValueTransformer : DynamicRouteValueTransformer
{
    private ILogger<WebRouteValueTransformer> _logger;

    /// <summary>
    /// 
    /// </summary>
    public WebRouteValueTransformer(ILogger<WebRouteValueTransformer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
    {
        var route = httpContext?.Request?.Path.Value?.ToLower();

        // Check for Blog Links
        if (route == "/blog")
        {
            values["controller"] = "Blog";
            values["action"] = "index";
            values["id"] = string.Empty;
            return new ValueTask<RouteValueDictionary>(values);
        }
        if (route?.StartsWith("/posts/") == true)
        {
            var slug = route.Replace("/posts/", string.Empty);
            values["controller"] = "Blog";
            values["action"] = "single";
            values["id"] = slug;
            return new ValueTask<RouteValueDictionary>(values);
        }
        if (route?.StartsWith("/categories/") == true)
        {
            var slug = route.Replace("/categories/", string.Empty);
            values["controller"] = "Blog";
            values["action"] = "categories";
            values["id"] = slug;
            return new ValueTask<RouteValueDictionary>(values);
        }

        // Admin
        if (route?.StartsWith("/admin/") == true)
        {
            var slug = route.Replace("/admin/", string.Empty);
            values["controller"] = "admin";
            values["action"] = "index";
            values["id"] = slug;
            return new ValueTask<RouteValueDictionary>(values);
        }

        var value = httpContext?.Session.GetString("BaseViewKey");
        var baseView = value == null ? default : JsonSerializer.Deserialize<WebsiteVM>(value);
        var myPage = baseView?.Menu?.Where(w => w.Url.ToLower() == httpContext?.Request?.Path.Value?.ToLower()).FirstOrDefault();
        if (myPage == null)
        {
            if (string.IsNullOrWhiteSpace(route?.Replace("/", string.Empty)))
            {
                // If this is the root, then return the default page (first in display order)
                myPage = baseView?.Menu?.Where(w => w.ParentId == null).OrderBy(o => o.DisplayOrder).FirstOrDefault();
            }
            else
            {
                _logger.LogWarning("Page not found", new { route, values });
                values["controller"] = "error";
                values["controller"] = "error";
                values["action"] = "notfound";
                values["id"] = route;
                return new ValueTask<RouteValueDictionary>(values);
            }
        }

        //values["controller"] = myPage?.Controller ?? "Page";
        //values["action"] = myPage?.Controller == "Page" ? "index" : splitAction is null ? "index" : splitAction[0];
        //values["id"] = splitAction is null ? "index" : splitAction.Length > 1 ? splitAction[1] : myPage?.Controller == "Page" ? myPage?.Action : string.Empty;
        //return new ValueTask<RouteValueDictionary>(values);

        values["controller"] = myPage?.Controller ?? "Page";
        values["action"] = myPage?.Action ?? "index";
        values["id"] = myPage?.Argument ?? myPage?.Action;
        return new ValueTask<RouteValueDictionary>(values);

    }
}
