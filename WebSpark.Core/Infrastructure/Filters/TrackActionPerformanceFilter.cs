using Microsoft.AspNetCore.Mvc.Filters;

namespace WebSpark.Core.Infrastructure.Filters;

/// <summary>
/// TrackActionPerformanceFilter
/// </summary>
public class TrackActionPerformanceFilter : IActionFilter
{
    private IDisposable? _hostScope;
    private readonly ILogger<TrackActionPerformanceFilter> _logger;
    private readonly Interfaces.IScopeInformation _scopeInfo;
    private Stopwatch? _timer;
    private IDisposable? _userScope;

    /// <summary>
    /// TrackActionPerformanceFilter
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="scopeInfo"></param>
    public TrackActionPerformanceFilter(
        ILogger<TrackActionPerformanceFilter> logger,
        Interfaces.IScopeInformation scopeInfo)
    {
        _logger = logger;
        _scopeInfo = scopeInfo;
    }

    /// <summary>
    /// OnActionExecuted
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        _timer?.Stop();
        if (context.Exception == null)
        {
            if (_timer != null)
            {
                _logger.LogRoutePerformance(context.HttpContext.Request.Path,
                    context.HttpContext.Request.Method,
                    _timer.ElapsedMilliseconds);
            }
        }
        _userScope?.Dispose();
        _hostScope?.Dispose();
    }
    /// <summary>
    /// OnActionExecuting
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _timer = new Stopwatch();

        var userDict = new Dictionary<string, string?>
        {
            { "UserId", context.HttpContext.User.Claims.FirstOrDefault(a => a.Type == "sub")?.Value },
        { "OAuth2 Scopes", string.Join(",",
            context.HttpContext.User.Claims.Where(c => c.Type == "scope").Select(c => c.Value) ?? Array.Empty<string>()) }
        };
        _userScope = _logger.BeginScope(userDict!);
        _hostScope = _logger.BeginScope(_scopeInfo.HostScopeInfo);

        _timer.Start();
    }
}
