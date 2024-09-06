using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using WebSpark.Core.Models;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers.Api;

[Route("api/[area]/[controller]")]
public class StatusController(ApplicationStatus applicationStatus) : AsyncSparkBaseController
{
    /// <summary>
    /// Returns Current Application Status
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("")]
    public ApplicationStatus Get()
    {
        return applicationStatus;
    }
}
