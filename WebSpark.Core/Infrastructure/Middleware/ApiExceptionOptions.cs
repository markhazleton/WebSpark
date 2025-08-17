
namespace WebSpark.Core.Infrastructure.Middleware;

/// <summary>
/// 
/// </summary>
public class ApiExceptionOptions
{
    /// <summary>
    /// 
    /// </summary>
    public Action<HttpContext, Exception, ApiError> AddResponseDetails { get; set; } = (_, _, _) => { };
    /// <summary>
    /// 
    /// </summary>
    public Func<Exception, LogLevel> DetermineLogLevel { get; set; } = _ => LogLevel.Error;
}
