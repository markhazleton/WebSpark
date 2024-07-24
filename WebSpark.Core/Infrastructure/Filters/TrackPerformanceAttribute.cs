using Microsoft.AspNetCore.Mvc.Filters;

namespace WebSpark.Core.Infrastructure.Filters;

/// <summary>
/// Track Performance
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="logger"></param>
public class TrackPerformance(ILogger<Filters.TrackPerformance> logger) : ActionFilterAttribute
{
    private readonly Stopwatch _timer = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _timer.Start();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        _timer.Stop();
        if (context.Exception == null)
        {
            logger.LogRoutePerformance(context.HttpContext.Request.Path,
                context.HttpContext.Request.Method,
                _timer.ElapsedMilliseconds);
        }
    }
}
