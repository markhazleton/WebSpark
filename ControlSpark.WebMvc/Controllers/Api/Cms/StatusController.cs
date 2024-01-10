using System.Reflection;

namespace ControlSpark.WebMvc.Controllers.Api.Cms;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="service"></param>
[Route("/status")]
public class StatusController(IWebsiteService service) : ApiBaseController
{
    private readonly IWebsiteService service = service;

    /// <summary>
    /// Returns Current Application Status
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ApplicationStatus Get()
    {
        return GetApplicationStatus();
    }
    /// <summary>
    /// GetApplicationStatus
    /// </summary>
    /// <returns></returns>
    protected ApplicationStatus GetApplicationStatus()
    {
        return new ApplicationStatus(Assembly.GetExecutingAssembly(), service);
    }


}
