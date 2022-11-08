namespace ControlSpark.WebMvc.Controllers.Api;

/// <summary>
/// AboutController
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AboutController : ControllerBase
{
    private readonly IAboutProvider _aboutProvider;

    /// <summary>
    /// AboutController
    /// </summary>
    /// <param name="aboutProvider"></param>
    public AboutController(IAboutProvider aboutProvider)
    {
        _aboutProvider = aboutProvider;
    }

    /// <summary>
    /// GetAbout
    /// </summary>
    [HttpGet]
    public async Task<AboutModel> GetAbout()
    {
        return await _aboutProvider.GetAboutModel();
    }
}
