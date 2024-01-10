namespace ControlSpark.WebMvc.Controllers.Api;

/// <summary>
/// AboutController
/// </summary>
/// <remarks>
/// AboutController
/// </remarks>
/// <param name="aboutProvider"></param>
[Route("api/[controller]")]
[ApiController]
public class AboutController(IAboutProvider aboutProvider) : ControllerBase
{
    private readonly IAboutProvider _aboutProvider = aboutProvider;

    /// <summary>
    /// GetAbout
    /// </summary>
    [HttpGet]
    public async Task<AboutModel> GetAbout()
    {
        return await _aboutProvider.GetAboutModel();
    }
}
