using HttpClientUtility.SendService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;

namespace HttpClientCrawler.Crawler;

public class SiteCrawler : ISiteCrawler
{
    private readonly IHubContext<CrawlHub> _hubContext;
    private readonly IHttpClientSendService _service;
    private readonly ILogger<SiteCrawler> _logger;

    public SiteCrawler(IHubContext<CrawlHub> hubContext, IHttpClientSendService httpClientService, ILogger<SiteCrawler> logger)
    {
        _hubContext = hubContext;
        _service = httpClientService;
        _logger = logger;
    }

    /// <summary>
    /// Crawls a single page and returns the result.
    /// </summary>
    private async Task<CrawlResult> CrawlPageAsync(string url, int depth, CancellationToken ct = default)
    {
        var crawlRequest = new CrawlResult
        {
            CacheDurationMinutes = 0,
            RequestPath = url,
            Iteration = depth
        };

        try
        {
            var response = await _service.HttpClientSendAsync((HttpClientSendRequest<string>)crawlRequest, ct).ConfigureAwait(false);
            crawlRequest = new CrawlResult(response);
        }
        catch (HttpRequestException ex)
        {
            crawlRequest.StatusCode = HttpStatusCode.ServiceUnavailable;
            _logger.LogError("Error accessing page: {url}. Exception: {ex.Message}", url, ex.Message);
        }
        catch (Exception ex)
        {
            crawlRequest.StatusCode = HttpStatusCode.InternalServerError;
            _logger.LogError("Error accessing page: {url}. Exception: {ex.Message}", url, ex.Message);
        }

        return crawlRequest;
    }

    /// <summary>
    /// Crawls a website starting from the given path.
    /// </summary>
    public async Task<ICollection<CrawlResult>> CrawlAsync(int maxNumberOfResults, string startPath, CancellationToken ct = default)
    {
        var linksToCrawl = new Queue<string>();
        var crawlResults = new ConcurrentDictionary<string, CrawlResult>();

        linksToCrawl.Enqueue(startPath);

        try
        {
            while (linksToCrawl.Count > 0 && crawlResults.Count <= maxNumberOfResults)
            {
                var link = linksToCrawl.Dequeue();
                var crawlResult = await CrawlPageAsync(link, depth: crawlResults.Count, ct: ct);
                crawlResults.TryAdd(link, crawlResult);

                EnqueueNewLinks(crawlResult, linksToCrawl, crawlResults);

                await NotifyClientsAsync($"Links to parse:{linksToCrawl.Count} Crawled:{crawlResults.Count}", ct);
            }
        }
        finally
        {
            await NotifyClientsAsync($"CrawlAsync Complete:Crawled:{crawlResults.Count} links", ct);
        }

        return crawlResults.Values;
    }

    /// <summary>
    /// Enqueues new links to be crawled.
    /// </summary>
    private static void EnqueueNewLinks(CrawlResult crawlResult, Queue<string> linksToCrawl, ConcurrentDictionary<string, CrawlResult> crawlResults)
    {
        foreach (var crawlLink in crawlResult.CrawlLinks)
        {
            if (!crawlResults.ContainsKey(crawlLink) && !linksToCrawl.Contains(crawlLink))
            {
                linksToCrawl.Enqueue(crawlLink);
            }
        }
    }

    /// <summary>
    /// Sends real-time updates to connected clients.
    /// </summary>
    private async Task NotifyClientsAsync(string message, CancellationToken ct)
    {
        await _hubContext.Clients.All.SendAsync("UrlFound", message, ct);
    }
}
