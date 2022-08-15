using System.Reflection;

namespace ControlSpark.WebMvc.Controllers.Api.Cms;

/// <summary>
/// 
/// </summary>
[Route("/status")]
public class StatusController : ApiBaseController
{
    private readonly IWebsiteService service;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="service"></param>
    public StatusController(IWebsiteService service)
    {
        this.service = service;
    }

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
