using Microsoft.AspNetCore.Authorization;

namespace ControlSpark.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsProvider _analyticsProvider;

    public AnalyticsController(IAnalyticsProvider analyticsProvider)
    {
        _analyticsProvider = analyticsProvider;
    }

    [Authorize]
    [HttpGet]
    public async Task<AnalyticsModel> GetAnalytics()
    {
        return await _analyticsProvider.GetAnalytics();
    }

    [Authorize]
    [HttpPut("displayPeriod/{typeId:int}")]
    public async Task<ActionResult<bool>> SaveDisplayPeriod(int typeId)
    {
        return await _analyticsProvider.SaveDisplayPeriod(typeId);
    }

    [Authorize]
    [HttpPut("displayType/{typeId:int}")]
    public async Task<ActionResult<bool>> SaveDisplayType(int typeId)
    {
        return await _analyticsProvider.SaveDisplayType(typeId);
    }
}
