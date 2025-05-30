using System.Text.Json;

namespace WebSpark.Core.Infrastructure.Middleware;

/// <summary>
/// 
/// </summary>
public class ApiExceptionMiddleware
{
    private readonly ILogger<ApiExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly ApiExceptionOptions _options;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="next"></param>
    /// <param name="logger"></param>
    public ApiExceptionMiddleware(ApiExceptionOptions options, RequestDelegate next,
        ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    private static string GetInnermostExceptionMessage(Exception exception)
    {
        if (exception.InnerException != null)
            return GetInnermostExceptionMessage(exception.InnerException);

        return exception.Message;
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var error = new ApiError
        {
            Id = Guid.NewGuid().ToString(),
            Status = (short)HttpStatusCode.InternalServerError,
            Title = "Some kind of error occurred in the API.  Please use the id and contact our " +
                    "support team if the problem persists."
        };

        _options.AddResponseDetails?.Invoke(context, exception, error);

        var innerExMessage = GetInnermostExceptionMessage(exception);

        var level = _options.DetermineLogLevel?.Invoke(exception) ?? LogLevel.Error;
        _logger.Log(level, exception, $"BADNESS!!! {innerExMessage} -- {{ErrorId}}.", error.Id);

        var result = JsonSerializer.Serialize(error); // Using System.Text.Json

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Write the JSON result to the response stream
        return context.Response.WriteAsync(result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context /* other dependencies */)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
}
