using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using WebSpark.Core.Models;

namespace WebSpark.Main.Controllers.Api;

/// <summary>
/// Base for all Api Controllers in this project
/// </summary>
[Produces("application/json")]
[ApiController]
public abstract class BaseApiController : Controller
{
    private readonly IMemoryCache _memoryCache;
    private const string CacheKey = "ApplicationStatusCache";

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="memoryCache"></param>
    public BaseApiController(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// GetApplicationStatus
    /// </summary>
    /// <returns></returns>
    protected ApplicationStatus GetApplicationStatus()
    {
        if (!_memoryCache.TryGetValue(CacheKey, out ApplicationStatus applicationStatus))
        {
            applicationStatus = new ApplicationStatus(Assembly.GetExecutingAssembly());
            _memoryCache.Set(CacheKey, applicationStatus, TimeSpan.FromHours(24));
        }
        return applicationStatus;
    }
}
