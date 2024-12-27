using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json;

namespace WebSpark.Web.Extensions;

/// <summary>
/// Represents a class that transforms route values for web routes.
/// </summary>
public class WebRouteValueTransformer(ILogger<WebRouteValueTransformer> logger) : DynamicRouteValueTransformer
{

    private static readonly Dictionary<string, (string Controller, string Action)> PredefinedRoutes = new()
    {
        { "/blog", (RouteConstants.Blog, "index") },
        { "/admin", (RouteConstants.Admin, "index") }
    };

    private static ValueTask<RouteValueDictionary> CreateErrorRoute(RouteValueDictionary values)
    {
        SetValues(values, RouteConstants.Error, "notfound", string.Empty);
        return new ValueTask<RouteValueDictionary>(values);
    }

    private WebsiteVM? GetBaseView(HttpContext httpContext)
    {
        try
        {
            var sessionData = httpContext.Session.GetString(RouteConstants.BaseViewSessionKey);
            return string.IsNullOrEmpty(sessionData) ? null : JsonSerializer.Deserialize<WebsiteVM>(sessionData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize session data.");
            return null;
        }
    }

    private bool HandleAdmin(string route, RouteValueDictionary values)
    {
        if (route.StartsWith("/admin/"))
        {
            var slug = route.Replace("/admin/", string.Empty);
            SetValues(values, RouteConstants.Admin, "index", slug);
            return true;
        }
        return false;
    }

    private ValueTask<RouteValueDictionary> HandleBaseView(HttpContext httpContext, string route, RouteValueDictionary values)
    {
        var baseView = GetBaseView(httpContext);
        var myPage = baseView?.Menu?.FirstOrDefault(w => w.Url == route);

        if (myPage == null)
        {
            if (string.IsNullOrWhiteSpace(route.Replace("/", string.Empty)))
            {
                myPage = baseView?.Menu?.Where(w => w.ParentId == null).OrderBy(o => o.DisplayOrder).FirstOrDefault();
            }
            else
            {
                logger.LogWarning("Page not found for route: {Route}", route);
                return CreateErrorRoute(values);
            }
        }
        SetValues(values, myPage?.Controller ?? "Page", myPage?.Action ?? "index", myPage?.Argument ?? myPage?.Action ?? string.Empty);
        return new ValueTask<RouteValueDictionary>(values);
    }

    private bool HandleBlogLinks(string route, RouteValueDictionary values)
    {
        if (route.StartsWith("/posts/"))
        {
            var slug = route.Replace("/posts/", string.Empty);
            SetValues(values, RouteConstants.Blog, "single", slug);
            return true;
        }
        if (route.StartsWith("/categories/"))
        {
            var slug = route.Replace("/categories/", string.Empty);
            SetValues(values, RouteConstants.Blog, "categories", slug);
            return true;
        }
        return false;
    }

    private bool HandleFileExtensions(string route, RouteValueDictionary values)
    {
        var fileExtensions = new Dictionary<string, string>
        {
            { ".php", "BlankPHP" },
            { ".js", "BlankJS" },
            { ".css", "BlankCss" },
            { ".png", "BlankPNG" },
            { ".jpg", "BlankJPG" },
            { ".jpeg", "BlankJPG" },
            { ".gif", "BlankGIF" }
        };

        foreach (var (extension, action) in fileExtensions)
        {
            if (route.EndsWith(extension))
            {
                SetValues(values, RouteConstants.EmptyFiles, action, string.Empty);
                return true;
            }
        }
        return false;
    }

    private bool HandlePredefinedRoutes(string route, RouteValueDictionary values)
    {
        if (PredefinedRoutes.TryGetValue(route, out var mapping))
        {
            SetValues(values, mapping.Controller, mapping.Action, string.Empty);
            return true;
        }
        return false;
    }

    private static void SetValues(RouteValueDictionary values, string controller, string action, string id)
    {
        values["controller"] = controller;
        values["action"] = action;
        values["id"] = id;
    }

    /// <summary>
    /// Transforms the route values asynchronously.
    /// </summary>
    /// <param name="httpContext">The current HttpContext.</param>
    /// <param name="values">The route values.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
    {
        string route = httpContext.Request.Path.Value ?? "/";
        if (string.IsNullOrEmpty(route))
        {
            logger.LogError("Route is null or empty");
            return CreateErrorRoute(values);
        }

        var baseView = GetBaseView(httpContext);
        var myPage = baseView?.Menu?.FirstOrDefault(w => w.Url == route)
            ?? baseView?.Menu?.FirstOrDefault(w => w.Argument?.ToLower() == route.Trim('/').ToLower());

        if (myPage == null)
        {
            logger.LogWarning("Page not found for route: {Route}", route);
            return CreateErrorRoute(values);
        }


        // Check if the argument matches but the route is different
        if (myPage?.Url != route)
        {
            logger.LogInformation("Redirecting from route {Route} to {TargetUrl}", route, myPage?.Url);

            // Perform the redirect
            httpContext.Response.Clear(); // Clear any existing response data
            httpContext.Response.StatusCode = StatusCodes.Status302Found; // Temporary redirect
            httpContext.Response.Headers["Location"] = myPage.Url; // Set the new location header

            // Flush the response and terminate
            httpContext.Response.CompleteAsync();
            return new ValueTask<RouteValueDictionary>([]);
        }


        if (HandlePredefinedRoutes(route, values)
            || HandleFileExtensions(route, values)
            || HandleBlogLinks(route, values)
            || HandleAdmin(route, values))
        {
            return new ValueTask<RouteValueDictionary>(values);
        }
        return HandleBaseView(httpContext, route, values);
    }

    private static class RouteConstants
    {
        public const string Admin = "admin";
        public const string BaseViewSessionKey = "BaseViewKey";
        public const string Blog = "Blog";
        public const string EmptyFiles = "EmptyFiles";
        public const string Error = "error";
    }
}
