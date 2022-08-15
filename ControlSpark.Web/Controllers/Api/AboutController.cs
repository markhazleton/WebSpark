namespace ControlSpark.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class AboutController : ControllerBase
{
    private readonly IAboutProvider _aboutProvider;

    public AboutController(IAboutProvider aboutProvider)
    {
        _aboutProvider = aboutProvider;
    }

    [HttpGet]
    public async Task<AboutModel> GetAbout()
    {
        return await _aboutProvider.GetAboutModel();
    }
}
