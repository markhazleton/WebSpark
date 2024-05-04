
namespace WebSpark.Core.Infrastructure;

/// <summary>
/// Log Message
/// </summary>
public static class LogMessages
{
    private static readonly Action<ILogger, string, string, long, Exception> _routePerformance;

    static LogMessages()
    {
        _routePerformance = LoggerMessage.Define<string, string, long>(LogLevel.Information, 0,
            "{RouteName} {Method} code took {ElapsedMilliseconds}.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="pageName"></param>
    /// <param name="method"></param>
    /// <param name="elapsedMilliseconds"></param>
    public static void LogRoutePerformance(this ILogger logger, string pageName, string method,
        long elapsedMilliseconds)
    {
        _routePerformance(logger, pageName, method, elapsedMilliseconds, null);
    }
}
