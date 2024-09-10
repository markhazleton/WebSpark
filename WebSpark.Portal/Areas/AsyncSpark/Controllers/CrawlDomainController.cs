using HttpClientCrawler.Crawler;
using HttpClientUtility.SendService;
using Microsoft.AspNetCore.SignalR;
using WebSpark.Portal.Areas.AsyncSpark.Models.CrawlDomain;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

public class CrawlDomainController(IHubContext<CrawlHub> hubContext, IHttpClientSendService service, ILogger<SiteCrawler> logger) : AsyncSparkBaseController
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
            model.CrawlResults = await _siteCrawler.CrawlAsync(model.MaxPagesCrawled, model.StartPath).ConfigureAwait(true);
        }
        finally
        {
            // Notify clients that crawling has finished
            model.IsCrawling = false;
            await hubContext.Clients.All.SendAsync("UrlFound", $"CrawlAsync Is Complete");

            // Pause for 3 seconds to allow clients to see the final results
            await Task.Delay(3000);
        }
        return View(model);
    }
}