namespace ControlSpark.Web.Controllers;

/// <summary>
/// SiteMap Controller  
/// </summary>
public class SitemapController : BaseController
{
    /// <summary>
    /// SiteMap Controller Constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="config"></param>
    /// <param name="websiteService"></param>
    public SitemapController(ILogger<SitemapController> logger, IConfiguration config, IWebsiteService websiteService) : base(
        logger,
        config,
        websiteService)
    {
    }

    /// <summary>
    /// Return Sitemap.xml
    /// </summary>
    /// <returns></returns>
    [Route("sitemap.xml")]
    [Produces("text/xml")]
    public IActionResult Sitemap()
    {
        var sitemap = this.BaseVM.GenerateSitemapXDocument();
        return Content(sitemap.Declaration + Environment.NewLine + sitemap, "text/xml");
    }
}


