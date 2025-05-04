using Microsoft.AspNetCore.SignalR;
using WebSpark.HttpClientUtility.Crawler;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

public class CrawlDomainController(
    SiteCrawler _siteCrawler,
    IHubContext<CrawlHub> hubContext,
    ILogger<SiteCrawler> logger) : AsyncSparkBaseController
{

    // Action method for the initial form load
    [HttpGet]
    public IActionResult Index()
    {
        return View(new CrawlDomainViewModel { MaxPagesCrawled = 500 });
    }

    // Action method to handle form submission
    [HttpPost]
    public async Task<IActionResult> Index(CrawlDomainViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Notify clients that crawling has started
        model.IsCrawling = true;
        await hubContext.Clients.All.SendAsync("UrlFound", $"CrawlAsync Is Started", ct).ConfigureAwait(false);

        try
        {
            CrawlerOptions crawlerOptions = new()
            {
                MaxPages = model.MaxPagesCrawled,
                MaxDepth = 3,
                RequestDelayMs = 10,
                SavePagesToDisk = false,
                OutputDirectory = null,
                UserAgent = "HttpClientCrawler/1.0",
                RespectRobotsTxt = false,
                ValidateHtml = false
            };


            // Start the crawling process
            model = await _siteCrawler.CrawlAsync(model.StartPath, crawlerOptions).ConfigureAwait(true);

            model.Sitemap = System.Net.WebUtility.HtmlEncode(model.Sitemap);
        }
        finally
        {
            // Notify clients that crawling has finished

            model.IsCrawling = false;
            await hubContext.Clients.All.SendAsync("UrlFound", $"CrawlAsync Is Complete", ct).ConfigureAwait(false);
        }
        return View(model);
    }
}
