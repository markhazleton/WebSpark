using Microsoft.AspNetCore.Mvc.Filters;

namespace ControlSpark.Core.Infrastructure.Attributes;

/// <summary>
/// Track Performance
/// </summary>
public class TrackPerformance : ActionFilterAttribute
{
    private readonly ILogger<TrackPerformance> _logger;
    private readonly Stopwatch _timer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public TrackPerformance(ILogger<TrackPerformance> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

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
            _logger.LogRoutePerformance(context.HttpContext.Request.Path,
                context.HttpContext.Request.Method,
                _timer.ElapsedMilliseconds);
        }
    }
}
