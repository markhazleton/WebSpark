namespace WebSpark.Portal.Utilities;

public class NotFoundMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Response.StatusCode == 404)
        {
            context.Response.StatusCode = 200; // Reset status code to avoid loop

            var requestPath = context.Request.Path.ToString().ToLower();

            if (IsRedirectLoop(context))
            {
                return; // Stop if redirect loop detected
            }

            if (requestPath.StartsWith("/openai/"))
            {
                var newPath = requestPath.Replace("/openai/", "/promptspark/");
                RedirectWithProtection(context, newPath);
                return;
            }
            if (requestPath.StartsWith("/async/"))
            {
                var newPath = requestPath.Replace("/async/", "/asyncspark/");
                RedirectWithProtection(context, newPath);
                return;
            }

            var redirects = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "github", "/asyncspark/github" },
                { "promptspark", "/promptspark" },
                { "asyncspark", "/asyncspark" },
                { "dataspark", "/dataspark" },
                { "prompt", "/promptspark" },
                { "async", "/asyncspark" },
            };

            foreach (var redirect in redirects)
            {
                if (requestPath.Contains(redirect.Key))
                {
                    RedirectWithProtection(context, redirect.Value);
                    return;
                }
            }
        }
    }

    private void RedirectWithProtection(HttpContext context, string newPath)
    {
        if (!context.Request.Path.Equals(newPath, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers.Add("X-Redirected", "true");
            context.Response.Redirect(newPath, permanent: true);
        }
    }

    private bool IsRedirectLoop(HttpContext context)
    {
        if (context.Request.Headers.ContainsKey("Referer"))
        {
            var referer = context.Request.Headers["Referer"].ToString();
            if (referer.Contains(context.Request.Path, StringComparison.OrdinalIgnoreCase))
            {
                return true; // Redirect loop detected
            }
        }

        if (context.Response.Headers.ContainsKey("X-Redirected"))
        {
            return true; // Redirect loop detected
        }

        return false;
    }
}
