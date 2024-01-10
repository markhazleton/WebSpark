namespace ControlSpark.WebMvc.Controllers
{
    public abstract class BaseController(ILogger<BaseController> logger) : Controller
    {
        protected readonly ILogger<BaseController> _logger = logger;
    }
}