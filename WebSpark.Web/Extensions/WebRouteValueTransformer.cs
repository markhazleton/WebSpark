using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json;

namespace WebSpark.Web.Extensions;

/// <summary>
/// Represents a class that transforms route values for web routes.
/// </summary>
public class WebRouteValueTransformer(ILogger<WebRouteValueTransformer> logger) : DynamicRouteValueTransformer
{
    /// <summary>
    /// Transforms the route values asynchronously.
    /// </summary>
    /// <param name="httpContext">The current HttpContext.</param>
    /// <param name="values">The route values.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
    {
        var route = httpContext?.Request?.Path.Value?.ToLower();
        if (string.IsNullOrEmpty(route))
        {
            logger.LogError("Route is null or empty");
            values["controller"] = "error";
            values["action"] = "notfound";
            values["id"] = string.Empty;
            return new ValueTask<RouteValueDictionary>(values);
        }

        if (HandleFileExtensions(route, values)
            || HandleBlogLinks(route, values)
            || HandleAdmin(route, values))
        {
            return new ValueTask<RouteValueDictionary>(values);
        }
        return HandleBaseView(httpContext, route, values);
    }

    /// <summary>
    /// Handles the file extensions in the route.
    /// </summary>
    /// <param name="route">The route value.</param>
    /// <param name="values">The route values.</param>
    /// <returns><c>true</c> if the file extension is handled; otherwise, <c>false</c>.</returns>
    private bool HandleFileExtensions(string route, RouteValueDictionary values)
    {
        if (route.EndsWith(".php"))
        {
            SetValues(values, "EmptyFiles", "BlankPHP", string.Empty);
            return true;
        }
        if (route.EndsWith(".js"))
        {
            SetValues(values, "EmptyFiles", "BlankJS", string.Empty);
            return true;
        }
        if (route.EndsWith(".css"))
        {
            SetValues(values, "EmptyFiles", "BlankCss", string.Empty);
            return true;
        }
        if (route.EndsWith(".png"))
        {
            SetValues(values, "EmptyFiles", "BlankPNG", string.Empty);
            return true;
        }
        if (route.EndsWith(".jpg") || route.EndsWith(".jpeg"))
        {
            SetValues(values, "EmptyFiles", "BlankJPG", string.Empty);
            return true;
        }
        if (route.EndsWith(".gif"))
        {
            SetValues(values, "EmptyFiles", "BlankGIF", string.Empty);
            return true;
        }
        return false;
    }


    /// <summary>
    /// Handles the blog links in the route.
    /// </summary>
    /// <param name="route">The route value.</param>
    /// <param name="values">The route values.</param>
    /// <returns><c>true</c> if the blog link is handled; otherwise, <c>false</c>.</returns>
    private bool HandleBlogLinks(string route, RouteValueDictionary values)
    {
        if (route == "/blog")
        {
            SetValues(values, "Blog", "index", string.Empty);
            return true;
        }
        if (route.StartsWith("/posts/"))
        {
            var slug = route.Replace("/posts/", string.Empty);
            SetValues(values, "Blog", "single", slug);
            return true;
        }
        if (route.StartsWith("/categories/"))
        {
            var slug = route.Replace("/categories/", string.Empty);
            SetValues(values, "Blog", "categories", slug);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Handles the admin routes in the route.
    /// </summary>
    /// <param name="route">The route value.</param>
    /// <param name="values">The route values.</param>
    /// <returns><c>true</c> if the admin route is handled; otherwise, <c>false</c>.</returns>
    private bool HandleAdmin(string route, RouteValueDictionary values)
    {
        if (route.StartsWith("/admin/"))
        {
            var slug = route.Replace("/admin/", string.Empty);
            SetValues(values, "admin", "index", slug);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Handles the base view route.
    /// </summary>
    /// <param name="httpContext">The current HttpContext.</param>
    /// <param name="route">The route value.</param>
    /// <param name="values">The route values.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private ValueTask<RouteValueDictionary> HandleBaseView(HttpContext httpContext, string route, RouteValueDictionary values)
    {
        var value = httpContext?.Session.GetString("BaseViewKey");
        var baseView = string.IsNullOrEmpty(value) ? null : JsonSerializer.Deserialize<WebsiteVM>(value);
        var myPage = baseView?.Menu?.FirstOrDefault(w => w.Url == route);

        if (myPage == null)
        {
            if (string.IsNullOrWhiteSpace(route.Replace("/", string.Empty)))
            {
                myPage = baseView?.Menu?.Where(w => w.ParentId == null).OrderBy(o => o.DisplayOrder).FirstOrDefault();
            }
            else
            {
                logger.LogWarning("Page not found for route: {route}", route);
                SetValues(values, "error", "notfound", route);
                return new ValueTask<RouteValueDictionary>(values);
            }
        }

        SetValues(values, myPage?.Controller ?? "Page", myPage?.Action ?? "index", myPage?.Argument ?? myPage?.Action);
        return new ValueTask<RouteValueDictionary>(values);
    }

    /// <summary>
    /// Sets the controller, action, and id values in the route values.
    /// </summary>
    /// <param name="values">The route values.</param>
    /// <param name="controller">The controller value.</param>
    /// <param name="action">The action value.</param>
    /// <param name="id">The id value.</param>
    private static void SetValues(RouteValueDictionary values, string controller, string action, string id)
    {
        values["controller"] = controller;
        values["action"] = action;
        values["id"] = id;
    }
}
