using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace HttpClientCrawler.Crawler;

/// <summary>
/// A simplified implementation of ISiteCrawler with local page saving capabilities
/// </summary>
public class SimpleSiteCrawler : ISiteCrawler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SimpleSiteCrawler> _logger;
    private readonly RobotsTxtParser _robotsTxtParser;

    /// <summary>
    /// Initializes a new instance of the SimpleSiteCrawler
    /// </summary>
    public SimpleSiteCrawler(IHttpClientFactory httpClientFactory, ILogger<SimpleSiteCrawler> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _robotsTxtParser = new RobotsTxtParser(httpClientFactory, "HttpClientCrawler/1.0", logger);
    }

    /// <summary>
    /// Initializes domain-specific data before crawling
    /// </summary>
    private async Task InitializeDomainAsync(string domainUrl, CrawlerOptions options, CancellationToken ct = default)
    {
        try
        {
            // Process robots.txt if enabled
            if (options.RespectRobotsTxt)
            {
                await _robotsTxtParser.ProcessRobotsTxtAsync(domainUrl, ct);
            }

            // Process sitemap.xml if available
            await ProcessSitemapAsync(domainUrl, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing domain {Domain}", domainUrl);
        }
    }

    /// <summary>
    /// Attempts to fetch and process the site's sitemap.xml
    /// </summary>
    private async Task ProcessSitemapAsync(string domainUrl, CancellationToken ct = default)
    {
        var sitemapUrl = new Uri(new Uri(domainUrl), "sitemap.xml").ToString();
        try
        {
            using var httpClient = _httpClientFactory.CreateClient("SimpleSiteCrawler");
            using var response = await httpClient.GetAsync(sitemapUrl, ct);

            if (response.IsSuccessStatusCode)
            {
                var sitemapContent = await response.Content.ReadAsStringAsync(ct);
                var sitemapLinks = ParseSitemap(sitemapContent);

                _logger.LogInformation("Sitemap found at {Url} with {Count} links", sitemapUrl, sitemapLinks.Count);
                return;
            }

            _logger.LogInformation("Sitemap not found at {Url}", sitemapUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing sitemap at {Url}", sitemapUrl);
        }
    }

    /// <summary>
    /// Parses sitemap.xml content and extracts URLs
    /// </summary>
    private List<string> ParseSitemap(string sitemapContent)
    {
        // Return an empty list if the input is null or whitespace
        if (string.IsNullOrWhiteSpace(sitemapContent))
        {
            return [];
        }

        var links = new List<string>();

        try
        {
            var xdoc = XDocument.Parse(sitemapContent);

            // Ensure xdoc.Root is not null before proceeding
            if (xdoc.Root == null)
            {
                _logger.LogWarning("The XML document root is null");
                return links;
            }

            // Define namespace (nullable)
            XNamespace ns = xdoc.Root.GetDefaultNamespace();

            // Safely parse URLs with null checks
            var urlElements = xdoc.Root.Elements(ns + "url");
            foreach (var urlElement in urlElements)
            {
                var locElement = urlElement.Element(ns + "loc");
                if (locElement != null && !string.IsNullOrWhiteSpace(locElement.Value))
                {
                    links.Add(locElement.Value);
                }
            }
        }
        catch (XmlException ex)
        {
            // Handle specific XML parsing errors
            _logger.LogWarning(ex, "XML error parsing sitemap");
        }
        catch (Exception ex)
        {
            // Generic exception handler for other unexpected errors
            _logger.LogWarning(ex, "Unexpected error parsing sitemap");
        }

        return links;
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
    /// Crawls a single page and returns the result
    /// </summary>
    private async Task<CrawlResult?> CrawlPageAsync(string url, int depth, string userAgent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var stopwatch = Stopwatch.StartNew();
        var crawlResult = new CrawlResult
        {
            RequestPath = url,
            Depth = depth,
            FoundUrl = url,
            Id = Guid.NewGuid().GetHashCode()
        };

        try
        {
            var httpClient = _httpClientFactory.CreateClient("SimpleSiteCrawler");

            // Set user agent if provided
            if (!string.IsNullOrEmpty(userAgent))
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            }

            var response = await httpClient.GetAsync(url, ct);
            crawlResult.StatusCode = response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                crawlResult.ResponseResults = await response.Content.ReadAsStringAsync(ct);
                _logger.LogDebug("Successfully crawled {Url}", url);
            }
            else
            {
                _logger.LogInformation("HTTP {StatusCode} from {Url}", response.StatusCode, url);
            }
        }
        catch (HttpRequestException ex)
        {
            crawlResult.StatusCode = ex.StatusCode ?? HttpStatusCode.InternalServerError;
            crawlResult.Errors.Add($"HTTP error: {ex.Message}");
            _logger.LogWarning(ex, "HTTP error crawling {Url}", url);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            crawlResult.StatusCode = HttpStatusCode.RequestTimeout;
            crawlResult.Errors.Add($"Timeout error: {ex.Message}");
            _logger.LogWarning(ex, "Timeout crawling {Url}", url);
        }
        catch (Exception ex)
        {
            crawlResult.StatusCode = HttpStatusCode.InternalServerError;
            crawlResult.Errors.Add($"General error: {ex.Message}");
            _logger.LogError(ex, "Error crawling {Url}", url);
        }
        finally
        {
            stopwatch.Stop();
            crawlResult.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            crawlResult.CompletionDate = DateTime.Now;
        }

        return crawlResult;
    }

    /// <summary>
    /// Normalizes URLs to avoid duplicates caused by case sensitivity or trailing slashes
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
    /// Saves HTML content to disk
    /// </summary>
    private async Task SavePageToDiskAsync(CrawlResult result, string? outputDirectory)
    {
        if (result == null || string.IsNullOrWhiteSpace(result.ResponseResults))
        {
            _logger.LogDebug("No content to save or result is null");
            return;
        }

        try
        {
            string directoryPath = string.IsNullOrWhiteSpace(outputDirectory)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pages")
                : outputDirectory;

            Directory.CreateDirectory(directoryPath);

            string safeFileName = GetSafeFileName(result.RequestPath, directoryPath);
            string filePath = Path.Combine(directoryPath, safeFileName);

            string updatedHtmlContent = ResolveRelativeLinks(result.ResponseResults, result.RequestPath);

            // Validate HTML if required
            var validationMessages = ValidateHtml(updatedHtmlContent);
            if (validationMessages != null && validationMessages.Any())
            {
                _logger.LogInformation("Validation issues found for {Url}: {Issues}",
                    result.RequestPath, string.Join("; ", validationMessages));
            }

            // Ensure directory exists
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, updatedHtmlContent);
            _logger.LogDebug("Saved {Url} to {FilePath}", result.RequestPath, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving page {Url} to disk", result.RequestPath);
        }
    }

    /// <summary>
    /// Converts a URL to a safe filename for saving on disk
    /// </summary>
    private static string GetSafeFileName(string url, string outputDir)
    {
        try
        {
            var uri = new Uri(url);
            string path = uri.AbsolutePath.Split('?', '#')[0];
            path = string.IsNullOrEmpty(path) || path == "/" ? "index.html" : path;

            if (!path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                path = path.TrimEnd('/') + ".html";
            }

            // Replace invalid characters and limit length
            path = path.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            path = path.Length <= 150 ? path : path[..150];

            return path;
        }
        catch
        {
            return $"page_{Guid.NewGuid().ToString("N")}.html";
        }
    }

    /// <summary>
    /// Resolves relative links in HTML content to absolute URLs
    /// </summary>
    private static string ResolveRelativeLinks(string htmlContent, string baseUrl)
    {
        try
        {
            var baseUri = new Uri(baseUrl);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var nodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]|//img[@src]|//link[@href]|//script[@src]|//iframe[@src]");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    string attributeName = node.Name switch
                    {
                        "a" or "link" => "href",
                        _ => "src"
                    };

                    string originalValue = node.GetAttributeValue(attributeName, string.Empty);

                    if (!string.IsNullOrEmpty(originalValue) && Uri.TryCreate(originalValue, UriKind.RelativeOrAbsolute, out var relativeUri))
                    {
                        if (!relativeUri.IsAbsoluteUri)
                        {
                            var absoluteUri = new Uri(baseUri, relativeUri);
                            node.SetAttributeValue(attributeName, absoluteUri.AbsoluteUri);
                        }
                    }
                }
            }
            return htmlDoc.DocumentNode.OuterHtml;
        }
        catch (Exception)
        {
            return htmlContent; // Return original content if transformation fails
        }
    }

    /// <summary>
    /// Validates HTML content for basic issues
    /// </summary>
    private static List<string>? ValidateHtml(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return null;
        }

        List<string> messages = [];

        try
        {
            var htmlDoc = new HtmlDocument
            {
                OptionCheckSyntax = true
            };
            htmlDoc.LoadHtml(htmlContent);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                messages.AddRange(htmlDoc.ParseErrors
                    .Select(error => $"Line {error.Line}: {error.Reason}")
                    .Take(10)); // Limit to first 10 errors
            }

            // Check for images without alt attributes
            var imgNodes = htmlDoc.DocumentNode.SelectNodes("//img[not(@alt)]");
            if (imgNodes != null)
            {
                messages.Add($"Found {imgNodes.Count} image tags without alt attributes");
            }

            return messages.Count == 0 ? null : messages;
        }
        catch
        {
            return ["Unable to parse HTML content"];
        }
    }

    /// <summary>
    /// Crawls a website starting from the given URL with the specified options
    /// </summary>
    public async Task<CrawlDomainViewModel> CrawlAsync(string startUrl, CrawlerOptions options, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(startUrl))
        {
            throw new ArgumentException("Start URL cannot be null or empty", nameof(startUrl));
        }

        // Use default options if none provided
        options ??= new CrawlerOptions();

        var crawledURLs = new HashSet<string>();
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
            // Initialize domain data
            await InitializeDomainAsync(startUrl, options, ct);

            // Add start URL to queue
            string normalizedStartUrl = NormalizeUrl(startUrl);
            linksToCrawl.Enqueue((normalizedStartUrl, 1));

            // Process the queue
            while (linksToCrawl.TryDequeue(out var item) &&
                   crawlResults.Count < options.MaxPages &&
                   !ct.IsCancellationRequested)
            {
                var (url, depth) = item;

                // Skip if already crawled
                if (crawledURLs.Contains(url))
                {
                    continue;
                }

                // Skip if depth exceeds max
                if (depth > options.MaxDepth)
                {
                    continue;
                }

                // Mark as crawled
                crawledURLs.Add(url);

                // Add delay between requests
                await DelayRequestAsync(options.RequestDelayMs, ct);

                // Crawl the page
                var result = await CrawlPageAsync(url, depth, options.UserAgent, ct);
                if (result != null)
                {
                    // Add to results
                    crawlResults.TryAdd(url, result);

                    // Process links if status code is OK
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        // Process found links
                        foreach (var foundUrl in result.CrawlLinks)
                        {
                            try
                            {
                                var normalizedUrl = NormalizeUrl(foundUrl);

                                // Skip if empty or already processed
                                if (string.IsNullOrEmpty(normalizedUrl) ||
                                    crawledURLs.Contains(normalizedUrl) ||
                                    crawlResults.ContainsKey(normalizedUrl))
                                {
                                    continue;
                                }

                                // Check robots.txt if enabled
                                if (options.RespectRobotsTxt && !_robotsTxtParser.IsAllowed(normalizedUrl))
                                {
                                    _logger.LogDebug("Skipping {Url} - disallowed by robots.txt", normalizedUrl);
                                    continue;
                                }

                                // Add to queue
                                linksToCrawl.Enqueue((normalizedUrl, depth + 1));
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error processing link: {Link}", foundUrl);
                            }
                        }
                    }

                    // Save page to disk if enabled
                    if (options.SavePagesToDisk && result.StatusCode == HttpStatusCode.OK)
                    {
                        await SavePageToDiskAsync(result, options.OutputDirectory);
                    }

                    // Log progress
                    if (crawlResults.Count % 10 == 0)
                    {
                        _logger.LogInformation("Crawled: {CrawledCount} Queue: {QueueCount} Current depth: {Depth}",
                            crawlResults.Count, linksToCrawl.Count, depth);
                    }
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
            _logger.LogInformation("Crawl complete, processed {Count} pages", crawlResults.Count);
        }

        // Populate view model
        viewModel.CrawlResults = crawlResults.Values.ToList();

        // Generate sitemap
        if (crawlResults.Any())
        {
            viewModel.Sitemap = GenerateSitemapXml(crawlResults.Values
                .Where(r => r.StatusCode == HttpStatusCode.OK)
                .Select(r => r.RequestPath));
        }

        return viewModel;
    }

    /// <summary>
    /// Generates a sitemap XML string from the given URLs
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

            // Create the sitemap document with the namespace
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
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
