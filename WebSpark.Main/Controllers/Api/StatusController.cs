
using Microsoft.Extensions.Caching.Memory;
using WebSpark.Core.Extensions;
using WebSpark.Core.Models;

namespace WebSpark.Main.Controllers.Api;

/// <summary>
/// Status Controller
/// </summary>
/// <remarks>
/// Status Controller
/// </remarks>
/// <param name="configuration"></param>
/// <param name="memoryCache"></param>
[Route("/status")]
public class StatusController(IConfiguration configuration, IMemoryCache memoryCache) : BaseApiController(memoryCache)
{
    /// <summary>
    /// Returns Current Application Status
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("")]
    public ApplicationStatus Get()
    {
        return GetApplicationStatus();
    }

    /// <summary>
    /// Get App Settings
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("appsettings")]
    public object GetAppSettings()
    {
        string[] TestIdList = configuration.GetStringList("Async:TestIds");
        string[] MissingList = configuration.GetStringList("Async:Bad");
        string[] MissingDefault = configuration.GetStringList("Async:Bad", "3,2,1");
        string[] MissingDefaultSingle = configuration.GetStringList("Async:Bad", "99");
        int[] TestIdIntList = configuration.GetIntList("Async:TestIds");
        int[] MissingIntList = configuration.GetIntList("Async:Bad");
        int[] MissingDefaultInt = configuration.GetIntList("Async:Bad", "3,2,1");
        int[] BadDefaultInt = configuration.GetIntList("Async:Bad", "bob,sam,1");
        return new { TestIdList, MissingList, MissingDefault, MissingDefaultSingle, TestIdIntList, MissingIntList, MissingDefaultInt, BadDefaultInt };
    }
}


