using HttpClientCrawler.Crawler;
using HttpClientUtility.RequestResult;
using Microsoft.AspNetCore.SignalR;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

public class CrawlDomainController(
    IHubContext<CrawlHub> hubContext,
    IHttpRequestResultService service,
    ILogger<SiteCrawler> logger) : AsyncSparkBaseController
{
    private readonly SiteCrawler _siteCrawler = new(hubContext, service, logger);

    // Action method for the initial form load
    [HttpGet]
    public IActionResult Index()
    {
        return View(new CrawlDomainViewModel { MaxPagesCrawled = 500 });
    }

    // Action method to handle form submission
    [HttpPost]
    public async Task<IActionResult> Index(CrawlDomainViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Notify clients that crawling has started
        model.IsCrawling = true;
        await hubContext.Clients.All.SendAsync("UrlFound", $"CrawlAsync Is Started");

        try
        {
            // Start the crawling process
            model = await _siteCrawler.CrawlAsync(model.MaxPagesCrawled, model.StartPath).ConfigureAwait(true);

            model.Sitemap = System.Net.WebUtility.HtmlEncode(model.Sitemap);
        }
        finally
        {
            // Notify clients that crawling has finished

            model.IsCrawling = false;
            await hubContext.Clients.All.SendAsync("UrlFound", $"CrawlAsync Is Complete");
        }
        return View(model);
    }
}
