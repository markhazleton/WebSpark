using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebSpark.Domain.Interfaces;

namespace WebSpark.Core.Infrastructure.BaseClasses;

/// <summary>
/// 
/// </summary>
public abstract class BasePageModel : PageModel
{
    private IDisposable _hostScope;
    private readonly ILogger _logger;
    private readonly IScopeInformation _scopeInfo;
    private readonly Stopwatch _timer;
    private IDisposable _userScope;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="scopeInfo"></param>
    public BasePageModel(ILogger logger, IScopeInformation scopeInfo)
    {
        _logger = logger;
        _scopeInfo = scopeInfo;
        _timer = new Stopwatch();
    }

    private static string MaskEmailAddress(string emailAddress)
    {
        var atIndex = emailAddress?.IndexOf('@');
        if (atIndex > 1)
        {
            return $"{emailAddress[0]}{emailAddress[1]}***{emailAddress.Substring(atIndex.Value)}";
        }
        return emailAddress;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
        _timer.Stop();
        _logger.LogRoutePerformance(context.ActionDescriptor.RelativePath,
            context.HttpContext.Request.Method,
            _timer.ElapsedMilliseconds);

        _userScope?.Dispose();
        _hostScope?.Dispose();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var userDict = new Dictionary<string, string>
        {
            { "UserId", context.HttpContext.User.FindFirst("sub")?.Value },
            { "GivenName", context.HttpContext.User.FindFirst("given_name")?.Value },
            { "Email", MaskEmailAddress(context.HttpContext.User.FindFirst("email")?.Value) }
        };

        _userScope = _logger.BeginScope(userDict);
        _hostScope = _logger.BeginScope(_scopeInfo.HostScopeInfo);

        _timer.Start();
    }
}
