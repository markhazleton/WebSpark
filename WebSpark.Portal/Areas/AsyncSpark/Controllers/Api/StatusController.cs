using WebSpark.Core.Models;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers.Api;

[Route("api/[area]/[controller]")]
public class StatusController(ApplicationStatus applicationStatus) : BaseAsyncSparkApiController
{
    /// <summary>
    /// Returns Current Application Status
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Route("")]
    public IActionResult Index()
    {
        return Ok(applicationStatus);
    }
}

