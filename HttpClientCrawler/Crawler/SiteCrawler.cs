using HttpClientUtility.RequestResult;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Xml.Linq;

namespace HttpClientCrawler.Crawler;

/// <summary>
/// View model representing the results of a crawl operation
/// </summary>
public class CrawlDomainViewModel
{
    /// <summary>
    /// Collection of crawl results from the operation
    /// </summary>
    public ICollection<CrawlResult> CrawlResults { get; set; } = [];

    /// <summary>
    /// Indicates if the crawler is currently running
    /// </summary>
    public bool IsCrawling { get; set; }

    /// <summary>
    /// Maximum number of pages crawled or to be crawled
    /// </summary>
    public int MaxPagesCrawled { get; set; } = 500;

    /// <summary>
    /// XML content of the generated sitemap
    /// </summary>
    public string Sitemap { get; set; } = string.Empty;

    /// <summary>
    /// The starting URL for the crawl operation
    /// </summary>
    public string StartPath { get; set; } = string.Empty;
}

/// <summary>
/// Implementation of the ISiteCrawler interface with SignalR support for real-time updates
/// </summary>
public class SiteCrawler : ISiteCrawler
{
    private readonly IHubContext<CrawlHub> _hubContext;
    private readonly IHttpRequestResultService _httpClientService;
    private readonly ILogger<SiteCrawler> _logger;
    private readonly RobotsTxtParser _robotsTxtParser;
    private readonly IHttpClientFactory _httpClientFactory;

    public IHubContext<CrawlHub> HubContext { get; }
    public IHttpRequestResultService Service { get; }
    public ILogger<SiteCrawler> Logger { get; }

    /// <summary>
    /// Initializes a new instance of the SiteCrawler
    /// </summary>
    public SiteCrawler(
        IHubContext<CrawlHub> hubContext,
        IHttpRequestResultService httpClientService,
        IHttpClientFactory httpClientFactory,
        ILogger<SiteCrawler> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _robotsTxtParser = new RobotsTxtParser(httpClientFactory, "HttpClientCrawler/1.0", logger);
    }

    /// <summary>
    /// Public constructor for SiteCrawler
    /// </summary>
    /// <param name="hubContext"></param>
    /// <param name="service"></param>
    /// <param name="logger"></param>
    public SiteCrawler(IHubContext<CrawlHub> hubContext, IHttpRequestResultService service, ILogger<SiteCrawler> logger)
    {
        HubContext = hubContext;
        Service = service;
        Logger = logger;
    }

    /// <summary>
    /// Crawls a single page and returns the result.
    /// </summary>
    private async Task<CrawlResult> CrawlPageAsync(string url, int depth, string userAgent, CancellationToken ct = default)
    {
        var crawlRequest = new CrawlResult
        {
            CacheDurationMinutes = 0,
            RequestPath = url,
            Iteration = depth,
            Depth = depth
        };

        try
        {
            var httpClient = _httpClientFactory.CreateClient("SiteCrawler");
            if (!string.IsNullOrEmpty(userAgent))
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            }

            var response = await _httpClientService.HttpSendRequestResultAsync((HttpRequestResult<string>)crawlRequest, ct: ct).ConfigureAwait(false);
            crawlRequest = new CrawlResult(response)
            {
                Depth = depth,
                FoundUrl = url
            };
        }
        catch (HttpRequestException ex)
        {
            crawlRequest.StatusCode = ex.StatusCode ?? HttpStatusCode.ServiceUnavailable;
            _logger.LogWarning("HttpRequestException accessing page: {Url}. StatusCode: {StatusCode}. Exception: {Message}",
                url, crawlRequest.StatusCode, ex.Message);
            crawlRequest.Errors.Add($"HTTP error: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            crawlRequest.StatusCode = HttpStatusCode.RequestTimeout;
            _logger.LogWarning("Timeout accessing page: {Url}. Exception: {Message}", url, ex.Message);
            crawlRequest.Errors.Add($"Timeout error: {ex.Message}");
        }
        catch (Exception ex)
        {
            crawlRequest.StatusCode = HttpStatusCode.InternalServerError;
            _logger.LogError(ex, "Exception accessing page: {Url}", url);
            crawlRequest.Errors.Add($"General error: {ex.Message}");
        }

        return crawlRequest;
    }

    /// <summary>
    /// Enqueues new links to be crawled.
    /// </summary>
    private void EnqueueNewLinks(
        CrawlResult crawlResult,
        ConcurrentQueue<(string Url, int Depth)> linksToCrawl,
        ConcurrentDictionary<string, CrawlResult> crawlResults,
        CrawlerOptions options)
    {
        if (crawlResult?.CrawlLinks == null)
        {
            return;
        }

        foreach (var crawlLink in crawlResult.CrawlLinks)
        {
            try
            {
                var normalizedLink = NormalizeUrl(crawlLink);

                // Skip if already crawled or in queue, or if depth would exceed max
                if (string.IsNullOrEmpty(normalizedLink) ||
                    crawlResults.ContainsKey(normalizedLink) ||
                    linksToCrawl.Any(item => item.Url == normalizedLink) ||
                    crawlResult.Depth >= options.MaxDepth)
                {
                    continue;
                }

                // Check robots.txt if enabled
                if (options.RespectRobotsTxt && !_robotsTxtParser.IsAllowed(normalizedLink))
                {
                    _logger.LogInformation("Skipping {Url} - disallowed by robots.txt", normalizedLink);
                    continue;
                }

                linksToCrawl.Enqueue((normalizedLink, crawlResult.Depth + 1));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing link: {Link}", crawlLink);
            }
        }
    }

    /// <summary>
    /// Generates the sitemap XML string from the given URLs.
    /// </summary>
    private static string GenerateSitemapXml(IEnumerable<string> urls)
    {
        if (urls == null || !urls.Any())
        {
            return string.Empty;
        }

        try
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
        catch (Exception ex)
        {
            // Log but don't throw
            Console.WriteLine($"Error generating sitemap: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Normalizes URLs to avoid duplicates caused by case sensitivity or trailing slashes.
    /// </summary>
    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        try
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return uri.GetLeftPart(UriPartial.Path).TrimEnd('/').ToLowerInvariant() + uri.Query;
            }
            return url.Trim().TrimEnd('/').ToLowerInvariant();
        }
        catch
        {
            return url.Trim().TrimEnd('/').ToLowerInvariant();
        }
    }

    /// <summary>
    /// Sends real-time updates to connected clients.
    /// </summary>
    private async Task NotifyClientsAsync(string message, CancellationToken ct)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("UrlFound", message, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification to clients");
        }
    }

    /// <summary>
    /// Adds a delay between requests to avoid overloading the server
    /// </summary>
    private static async Task DelayRequestAsync(int requestDelayMs, CancellationToken ct)
    {
        if (requestDelayMs > 0)
        {
            await Task.Delay(requestDelayMs, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Crawls a website starting from the given URL and returns the CrawlDomainViewModel.
    /// </summary>
    public async Task<CrawlDomainViewModel> CrawlAsync(string startUrl, CrawlerOptions options, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(startUrl))
        {
            throw new ArgumentException("Start URL cannot be null or empty", nameof(startUrl));
        }

        options ??= new CrawlerOptions();

        var linksToCrawl = new ConcurrentQueue<(string Url, int Depth)>();
        var crawlResults = new ConcurrentDictionary<string, CrawlResult>();
        var viewModel = new CrawlDomainViewModel
        {
            StartPath = startUrl,
            MaxPagesCrawled = options.MaxPages,
            IsCrawling = true
        };

        try
        {
            // Process robots.txt first if enabled
            if (options.RespectRobotsTxt)
            {
                await _robotsTxtParser.ProcessRobotsTxtAsync(startUrl, ct);
            }

            // Add the start URL to the queue
            linksToCrawl.Enqueue((NormalizeUrl(startUrl), 1));

            // Start crawling
            while (linksToCrawl.TryDequeue(out var item) &&
                   crawlResults.Count < options.MaxPages &&
                   !ct.IsCancellationRequested)
            {
                var (link, depth) = item;

                // Skip if already crawled
                if (crawlResults.ContainsKey(link))
                {
                    continue;
                }

                // Skip if depth exceeds max depth
                if (depth > options.MaxDepth)
                {
                    continue;
                }

                // Add delay between requests
                await DelayRequestAsync(options.RequestDelayMs, ct);

                // Crawl the page
                var crawlResult = await CrawlPageAsync(link, depth, options.UserAgent, ct);
                crawlResults.TryAdd(link, crawlResult);

                // Add new links to the queue
                EnqueueNewLinks(crawlResult, linksToCrawl, crawlResults, options);

                // Notify clients periodically
                if (crawlResults.Count % 10 == 0)
                {
                    await NotifyClientsAsync($"Links to parse: {linksToCrawl.Count} Crawled: {crawlResults.Count}", ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Crawl operation was cancelled");
            throw; // Re-throw to allow proper cancellation handling
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during crawling");
            throw; // Re-throw to allow proper exception handling
        }
        finally
        {
            viewModel.IsCrawling = false;
            await NotifyClientsAsync($"Crawl Complete: Crawled {crawlResults.Count} links", ct);
        }

        viewModel.CrawlResults = crawlResults.Values.ToList();
        viewModel.Sitemap = GenerateSitemapXml(crawlResults.Values
            .Where(r => r.StatusCode == HttpStatusCode.OK)
            .Select(result => result.RequestPath)
            .Distinct());

        return viewModel;
    }
}
