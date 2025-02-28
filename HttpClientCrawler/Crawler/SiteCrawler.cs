using HttpClientUtility.RequestResult;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Xml.Linq;

namespace HttpClientCrawler.Crawler;
public class CrawlDomainViewModel
{
    public ICollection<CrawlResult> CrawlResults { get; set; } = [];
    public bool IsCrawling { get; set; }
    public int MaxPagesCrawled { get; set; } = 500; // Default maximum pages for crawling
    public string Sitemap { get; set; } = string.Empty;
    public string StartPath { get; set; }
}
public class SiteCrawler(
    IHubContext<CrawlHub> hubContext,
    IHttpRequestResultService httpClientService,
    ILogger<SiteCrawler> logger) : ISiteCrawler
{

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
            var response = await httpClientService.HttpSendRequestResultAsync((HttpRequestResult<string>)crawlRequest, ct: ct).ConfigureAwait(false);
            crawlRequest = new CrawlResult(response);
        }
        catch (HttpRequestException ex)
        {
            crawlRequest.StatusCode = HttpStatusCode.ServiceUnavailable;
            logger.LogError("HttpRequestException accessing page: {url}. Exception: {ex.Message}", url, ex.Message);
        }
        catch (Exception ex)
        {
            crawlRequest.StatusCode = HttpStatusCode.InternalServerError;
            logger.LogError("Exception accessing page: {url}. Exception: {ex.Message}. StackTrace: {ex.StackTrace}", url, ex.Message, ex.StackTrace);
        }

        return crawlRequest;
    }


    /// <summary>
    /// Enqueues new links to be crawled.
    /// </summary>
    private static void EnqueueNewLinks(CrawlResult crawlResult, ConcurrentQueue<string> linksToCrawl, ConcurrentDictionary<string, CrawlResult> crawlResults)
    {
        foreach (var crawlLink in crawlResult.CrawlLinks)
        {
            var normalizedLink = NormalizeUrl(crawlLink);
            if (!crawlResults.ContainsKey(normalizedLink) && !linksToCrawl.Contains(normalizedLink))
            {
                linksToCrawl.Enqueue(normalizedLink);
            }
        }
    }

    /// <summary>
    /// Generates the sitemap XML string from the given URLs.
    /// </summary>
    private static string GenerateSitemapXml(IEnumerable<string> urls)
    {
        // Define the namespace for the sitemap
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        // Create the sitemap document with the namespace defined only at the root level
        var sitemap = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement(ns + "urlset",
                urls.Select(url => new XElement(ns + "url",
                    new XElement(ns + "loc", url),
                    new XElement(ns + "lastmod", DateTime.UtcNow.ToString("yyyy-MM-dd")),
                    new XElement(ns + "changefreq", "weekly"),
                    new XElement(ns + "priority", "0.5")))));

        return sitemap.ToString();
    }

    /// <summary>
    /// Normalizes URLs to avoid duplicates caused by case sensitivity or trailing slashes.
    /// </summary>
    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        return url.Trim().TrimEnd('/').ToLowerInvariant();
    }

    /// <summary>
    /// Sends real-time updates to connected clients.
    /// </summary>
    private async Task NotifyClientsAsync(string message, CancellationToken ct)
    {
        await hubContext.Clients.All.SendAsync("UrlFound", message, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Crawls a website starting from the given path and returns the CrawlDomainViewModel.
    /// </summary>
    public async Task<CrawlDomainViewModel> CrawlAsync(int maxNumberOfResults, string startPath, CancellationToken ct = default)
    {
        var linksToCrawl = new ConcurrentQueue<string>();
        var crawlResults = new ConcurrentDictionary<string, CrawlResult>();
        var viewModel = new CrawlDomainViewModel
        {
            StartPath = startPath,
            MaxPagesCrawled = maxNumberOfResults
        };

        linksToCrawl.Enqueue(NormalizeUrl(startPath));

        try
        {
            while (linksToCrawl.TryDequeue(out var link) && crawlResults.Count <= maxNumberOfResults)
            {
                if (ct.IsCancellationRequested)
                {
                    logger.LogInformation("CrawlAsync cancelled by user.");
                    ct.ThrowIfCancellationRequested();
                }

                var crawlResult = await CrawlPageAsync(link, depth: crawlResults.Count, ct: ct);
                crawlResults.TryAdd(link, crawlResult);

                EnqueueNewLinks(crawlResult, linksToCrawl, crawlResults);

                if (crawlResults.Count % 10 == 0) // Throttling client notifications
                {
                    await NotifyClientsAsync($"Links to parse: {linksToCrawl.Count} Crawled: {crawlResults.Count}", ct);
                }
            }
        }
        finally
        {
            await NotifyClientsAsync($"CrawlAsync Complete: Crawled: {crawlResults.Count} links", ct);
        }

        viewModel.CrawlResults = crawlResults.Values.ToList();
        viewModel.Sitemap = GenerateSitemapXml(crawlResults.Values.Select(result => result.RequestPath).Distinct());

        return viewModel;
    }
}
