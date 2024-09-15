namespace WebSpark.Portal.Utilities;

public class NotFoundMiddleware
{
    private readonly RequestDelegate _next;

    public NotFoundMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == 404)
        {
            var requestPath = context.Request.Path.ToString().ToLower();
            if (requestPath.Contains("promptspark"))
            {
                context.Response.Redirect("/promptspark", permanent: true);
            }
            if (requestPath.Contains("asyncspark"))
            {
                context.Response.Redirect("/asyncspark", permanent: true);
            }
            if (requestPath.Contains("dataspark"))
            {
                context.Response.Redirect("/dataspark", permanent: true);
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
