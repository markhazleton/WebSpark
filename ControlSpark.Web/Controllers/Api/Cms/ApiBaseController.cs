using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace ControlSpark.Web.Controllers.Api.Cms;

/// <summary>
/// Api Base Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
[TypeFilter(typeof(TrackPerformance))]
public class ApiBaseController : ControllerBase
{
    /// <summary>
    /// Api Base Controller constructor
    /// </summary>
    public ApiBaseController()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="ControllerName"></param>
    /// <returns></returns>
    protected string GetObjectUrl(object itemId = null, string ControllerName = null)
    {
        if (ControllerName == null)
        {
            var controllerActionDescriptor = HttpContext
                .GetEndpoint()
                .Metadata
                .GetMetadata<ControllerActionDescriptor>();
            ControllerName = controllerActionDescriptor.ControllerName;
        }
        return this.Url.ActionLink(action: null, controller: ControllerName, values: new { id = itemId });
    }
}
