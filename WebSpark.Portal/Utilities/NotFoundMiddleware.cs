namespace WebSpark.Portal.Utilities;

public class NotFoundMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Response.StatusCode == 404)
        {
            var requestPath = context.Request.Path.ToString().ToLower();
            if (requestPath.StartsWith("/openai/"))
            {
                var newPath = requestPath.Replace("/openai/", "/promptspark/");
                context.Response.Redirect(newPath, permanent: true);
                return;
            }
            if (requestPath.StartsWith("/async/"))
            {
                var newPath = requestPath.Replace("/async/", "/asyncspark/");
                context.Response.Redirect(newPath, permanent: true);
                return;
            }
            var redirects = new List<KeyValuePair<string, string>>
            {
                new("promptspark", "/promptspark"),
                new("asyncspark", "/asyncspark"),
                new("dataspark", "/dataspark"),
                new("prompt", "/promptspark"),
                new("async", "/asyncspark")
            };

            // Iterate through the list to find the first match and redirect
            foreach (var redirect in redirects)
            {
                if (requestPath.Contains(redirect.Key))
                {
                    context.Response.Redirect(redirect.Value, permanent: true);
                    break; // Exit after the first match
                }
            }
        }
    }
}
// Middleware to enforce lowercase routes
//app.Use(async (context, next) =>
//{
//    var request = context.Request;
//    var path = request.Path.Value;

//    // Check if the path contains any uppercase characters
//    if (path != null && path.Any(char.IsUpper))
//    {
//        // Convert the path to lowercase
//        var lowercasePath = path.ToLowerInvariant();

//        // Construct the new URL with the lowercase path
//        var newUrl = $"{request.Scheme}://{request.Host}{lowercasePath}{request.QueryString}";

//        // Redirect to the lowercase URL with a 301 (Permanent Redirect) status code
//        context.Response.Redirect(newUrl, true);
//        return;
//    }
//    await next();
//});
