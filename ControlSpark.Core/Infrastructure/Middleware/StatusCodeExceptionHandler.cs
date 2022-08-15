
namespace ControlSpark.Core.Infrastructure.Middleware;

/// <summary>
/// 
/// </summary>
public class StatusCodeExceptionHandler
{
    private readonly RequestDelegate request;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pipeline"></param>
    public StatusCodeExceptionHandler(RequestDelegate pipeline)
    {
        request = pipeline;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task Invoke(HttpContext context) => InvokeAsync(context); // Stops VS from nagging about async method without ...Async suffix.

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await request(context);
        }
        catch (StatusCodeException exception)
        {
            context.Response.StatusCode = (int)exception.StatusCode;
            context.Response.Headers.Clear();
        }
    }
}


