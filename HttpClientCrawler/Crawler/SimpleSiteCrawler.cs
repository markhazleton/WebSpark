using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace HttpClientCrawler.Crawler;

/// <summary>
/// A simplified implementation of ISiteCrawler with local page saving capabilities.
/// </summary>
/// <remarks>
/// This crawler provides efficient web crawling with features such as HTML content validation,
/// local file storage, and respect for robots.txt rules. It includes performance tracking
/// and memory usage optimization for large-scale crawling operations.
/// </remarks>
public class SimpleSiteCrawler : ISiteCrawler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SimpleSiteCrawler> _logger;
    private readonly RobotsTxtParser _robotsTxtParser;
    private readonly CrawlerPerformanceTracker _performanceTracker;

    /// <summary>
    /// Initializes a new instance of the SimpleSiteCrawler class.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients with appropriate configuration.</param>
    /// <param name="logger">Logger for diagnostic and operational information.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClientFactory or logger is null.</exception>
    public SimpleSiteCrawler(IHttpClientFactory httpClientFactory, ILogger<SimpleSiteCrawler> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _robotsTxtParser = new RobotsTxtParser(httpClientFactory, "HttpClientCrawler/1.0", logger);
        _performanceTracker = new CrawlerPerformanceTracker(logger);
    }

    /// <summary>
    /// Initializes domain-specific data before crawling.
    /// </summary>
    /// <param name="domainUrl">The URL of the domain to initialize.</param>
    /// <param name="options">Crawler configuration options.</param>
    /// <param name="ct">Cancellation token to stop the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="CrawlException">Thrown when domain initialization fails.</exception>
    private async Task InitializeDomainAsync(string domainUrl, CrawlerOptions options, CancellationToken ct = default)
    {
        try
        {
            // Process robots.txt if enabled
            if (options.RespectRobotsTxt)
            {
                await _performanceTracker.ExecuteAndTrackAsync(
                    "RobotsTxtProcessing",
                    () => _robotsTxtParser.ProcessRobotsTxtAsync(domainUrl, ct),
                    ct).ConfigureAwait(false);
            }

            // Process sitemap.xml if available
            await _performanceTracker.ExecuteAndTrackAsync(
                "SitemapProcessing",
                () => ProcessSitemapAsync(domainUrl, ct),
                ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing domain {Domain}", domainUrl);
            throw new CrawlException($"Failed to initialize domain: {ex.Message}", domainUrl, ex);
        }
    }

    /// <summary>
    /// Attempts to fetch and process the site's sitemap.xml.
    /// </summary>
    /// <param name="domainUrl">The URL of the domain to process the sitemap for.</param>
    /// <param name="ct">Cancellation token to stop the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessSitemapAsync(string domainUrl, CancellationToken ct = default)
    {
        var sitemapUrl = new Uri(new Uri(domainUrl), "sitemap.xml").ToString();
        try
        {
            using var httpClient = _httpClientFactory.CreateClient("SimpleSiteCrawler");
            // Set reasonable timeout to avoid hanging on slow responses
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            using var response = await httpClient.GetAsync(sitemapUrl, ct).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var sitemapContent = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var sitemapLinks = ParseSitemap(sitemapContent);

                _logger.LogInformation("Sitemap found at {Url} with {Count} links", sitemapUrl, sitemapLinks.Count);
                return;
            }

            _logger.LogInformation("Sitemap not found at {Url} (Status: {StatusCode})",
                sitemapUrl, response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error accessing sitemap at {Url}: {StatusCode}",
                sitemapUrl, ex.StatusCode);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout accessing sitemap at {Url}", sitemapUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing sitemap at {Url}", sitemapUrl);
        }
    }

    /// <summary>
    /// Parses sitemap.xml content and extracts URLs.
    /// </summary>
    /// <param name="sitemapContent">The XML content of the sitemap.</param>
    /// <returns>A list of URLs extracted from the sitemap.</returns>
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
    /// Adds a delay between requests to avoid overloading the server.
    /// </summary>
    /// <param name="requestDelayMs">Delay duration in milliseconds.</param>
    /// <param name="ct">Cancellation token to stop the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task DelayRequestAsync(int requestDelayMs, CancellationToken ct)
    {
        if (requestDelayMs > 0)
        {
            await Task.Delay(requestDelayMs, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Crawls a single page and returns the result.
    /// </summary>
    /// <param name="url">The URL to crawl.</param>
    /// <param name="depth">Current depth in the crawl hierarchy.</param>
    /// <param name="userAgent">User agent string to use for the request.</param>
    /// <param name="ct">Cancellation token to stop the operation.</param>
    /// <returns>A CrawlResult containing information about the crawled page, or null if the URL is invalid.</returns>
    /// <exception cref="CrawlException">Thrown when a critical error occurs during page crawling.</exception>
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

            // Set reasonable timeout to avoid hanging on slow responses
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Set user agent if provided
            if (!string.IsNullOrEmpty(userAgent))
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            }

            // Add standard headers for better compatibility
            httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.5");

            var response = await httpClient.GetAsync(url, ct).ConfigureAwait(false);
            crawlResult.StatusCode = response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                // Only attempt to read content for successful responses
                if (response.Content.Headers.ContentType?.MediaType?.StartsWith("text/", StringComparison.OrdinalIgnoreCase) == true)
                {
                    crawlResult.ResponseResults = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                    _logger.LogDebug("Successfully crawled {Url}", url);
                }
                else
                {
                    _logger.LogDebug("Skipping non-text content at {Url}, Content-Type: {ContentType}",
                        url, response.Content.Headers.ContentType?.MediaType);
                }
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

            // Track performance metrics
            _performanceTracker.TrackOperation("PageCrawl", stopwatch.ElapsedMilliseconds);
        }

        return crawlResult;
    }

    /// <summary>
    /// Normalizes URLs to avoid duplicates caused by case sensitivity or trailing slashes.
    /// </summary>
    /// <param name="url">The URL to normalize.</param>
    /// <returns>The normalized URL.</returns>
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
    /// Saves HTML content to disk.
    /// </summary>
    /// <param name="result">The crawl result containing the HTML content to save.</param>
    /// <param name="outputDirectory">The directory to save the HTML files to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="IOException">Thrown when there's an error writing files to disk.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the result parameter is null.</exception>
    private async Task SavePageToDiskAsync(CrawlResult result, string? outputDirectory)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result), "Crawl result cannot be null");
        }

        if (string.IsNullOrWhiteSpace(result.ResponseResults))
        {
            _logger.LogDebug("No content to save for {Url}", result.RequestPath);
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

            await File.WriteAllTextAsync(filePath, updatedHtmlContent).ConfigureAwait(false);
            _logger.LogDebug("Saved {Url} to {FilePath}", result.RequestPath, filePath);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error saving page {Url} to disk", result.RequestPath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving page {Url} to disk", result.RequestPath);
            throw new IOException($"Failed to save page to disk: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Converts a URL to a safe filename for saving on disk.
    /// </summary>
    /// <param name="url">The URL to convert to a filename.</param>
    /// <param name="outputDir">The output directory where the file will be saved.</param>
    /// <returns>A safe filename for the given URL.</returns>
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

            // Limit path length to avoid exceeding file system limits
            if (path.Length > 150)
            {
                string extension = Path.GetExtension(path);
                string fileName = Path.GetFileNameWithoutExtension(path);
                string directory = Path.GetDirectoryName(path) ?? string.Empty;

                // Trim the filename while preserving extension and directory structure
                fileName = fileName.Substring(0, Math.Min(fileName.Length, 150 - extension.Length - directory.Length - 1));
                path = Path.Combine(directory, fileName + extension);
            }

            return path;
        }
        catch
        {
            return $"page_{Guid.NewGuid().ToString("N")}.html";
        }
    }

    /// <summary>
    /// Resolves relative links in HTML content to absolute URLs.
    /// </summary>
    /// <param name="htmlContent">The HTML content to process.</param>
    /// <param name="baseUrl">The base URL to resolve relative links against.</param>
    /// <returns>HTML content with resolved links.</returns>
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
        catch (Exception ex)
        {
            // Log and return original content if transformation fails
            Debug.WriteLine($"Error resolving relative links: {ex.Message}");
            return htmlContent;
        }
    }

    /// <summary>
    /// Validates HTML content for basic issues.
    /// </summary>
    /// <param name="htmlContent">The HTML content to validate.</param>
    /// <returns>A list of validation messages, or null if no issues were found.</returns>
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
    /// Crawls a website starting from the given URL with the specified options.
    /// </summary>
    /// <param name="startUrl">The URL to start crawling from.</param>
    /// <param name="options">Options that control the crawling behavior.</param>
    /// <param name="ct">Cancellation token to stop the crawling process.</param>
    /// <returns>A view model containing the crawl results and sitemap.</returns>
    /// <exception cref="ArgumentException">Thrown when the start URL is null or empty.</exception>
    /// <exception cref="CrawlException">Thrown when a critical error occurs during crawling.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public async Task<CrawlDomainViewModel> CrawlAsync(string startUrl, CrawlerOptions options, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(startUrl))
        {
            throw new ArgumentException("Start URL cannot be null or empty", nameof(startUrl));
        }

        // Use default options if none provided
        options ??= new CrawlerOptions();

        // Estimate memory requirements and log warning if large crawl
        double estimatedMemoryMb = _performanceTracker.EstimateMemoryRequirements(options.MaxPages);
        if (estimatedMemoryMb > 500) // 500 MB
        {
            _logger.LogWarning("Large crawl detected for {Url}. Estimated memory: {MemoryMB:F2} MB",
                startUrl, estimatedMemoryMb);
        }

        var crawledURLs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var linksToCrawl = new ConcurrentQueue<(string Url, int Depth, int Priority)>();
        var crawlResults = new ConcurrentDictionary<string, CrawlResult>(StringComparer.OrdinalIgnoreCase);

        var viewModel = new CrawlDomainViewModel
        {
            StartPath = startUrl,
            MaxPagesCrawled = options.MaxPages,
            IsCrawling = true
        };

        try
        {
            // Initialize domain data
            await InitializeDomainAsync(startUrl, options, ct).ConfigureAwait(false);

            // Add start URL to queue
            string normalizedStartUrl = NormalizeUrl(startUrl);
            linksToCrawl.Enqueue((normalizedStartUrl, 1, 1)); // Priority 1 (highest) for start URL

            // Track number of URLs processed at each depth level for logging
            var depthStats = new ConcurrentDictionary<int, int>();

            // Process the queue with adaptive rate limiting
            int consecutiveTimeouts = 0;
            int currentDelay = options.RequestDelayMs;
            DateTime lastProgressLog = DateTime.Now;

            while (!linksToCrawl.IsEmpty &&
                   crawlResults.Count < options.MaxPages &&
                   !ct.IsCancellationRequested)
            {
                if (linksToCrawl.TryDequeue(out var item))
                {
                    var (url, depth, _) = item;

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

                    // Adaptive delay based on server response
                    if (consecutiveTimeouts > 3)
                    {
                        // Increase delay if we've had multiple timeouts
                        currentDelay = Math.Min(currentDelay * 2, 5000); // Cap at 5 seconds
                        _logger.LogWarning("Adaptive rate limit: Increased delay to {Delay}ms after {Timeouts} consecutive timeouts",
                            currentDelay, consecutiveTimeouts);
                    }

                    // Add delay between requests
                    await DelayRequestAsync(currentDelay, ct).ConfigureAwait(false);

                    // Crawl the page
                    var result = await CrawlPageAsync(url, depth, options.UserAgent, ct).ConfigureAwait(false);
                    if (result != null)
                    {
                        // Update timeout tracking
                        if (result.StatusCode == HttpStatusCode.RequestTimeout)
                        {
                            consecutiveTimeouts++;
                        }
                        else
                        {
                            // Reset consecutive timeout counter on success
                            if (consecutiveTimeouts > 0)
                            {
                                consecutiveTimeouts = 0;

                                // Gradually reduce delay back to original if we were in backoff mode
                                if (currentDelay > options.RequestDelayMs)
                                {
                                    currentDelay = Math.Max(currentDelay / 2, options.RequestDelayMs);
                                    _logger.LogInformation("Adaptive rate limit: Decreased delay to {Delay}ms after successful request",
                                        currentDelay);
                                }
                            }
                        }

                        // Add to results
                        crawlResults.TryAdd(url, result);

                        // Track depth stats
                        depthStats.AddOrUpdate(depth, 1, (_, count) => count + 1);

                        // Process links if status code is OK
                        if (result.StatusCode == HttpStatusCode.OK)
                        {
                            // Process found links
                            await ProcessFoundLinks(result, depth, crawledURLs, crawlResults, linksToCrawl, options, ct);
                        }

                        // Save page to disk if enabled
                        if (options.SavePagesToDisk && result.StatusCode == HttpStatusCode.OK)
                        {
                            await SavePageToDiskAsync(result, options.OutputDirectory).ConfigureAwait(false);
                        }

                        // Log progress periodically
                        if (crawlResults.Count % 10 == 0 || (DateTime.Now - lastProgressLog).TotalSeconds > 30)
                        {
                            lastProgressLog = DateTime.Now;
                            LogProgress(crawlResults.Count, linksToCrawl.Count, depth, depthStats);
                        }
                    }
                }
                else
                {
                    // If queue is empty but we didn't hit our MaxPages limit, wait a bit in case
                    // other threads are still processing and might add more links
                    await Task.Delay(100, ct).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Crawl operation was cancelled after processing {Count} pages",
                crawlResults.Count);
            throw; // Re-throw to allow proper cancellation handling
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during crawling of {Url}", startUrl);
            throw new CrawlException($"Crawl operation failed: {ex.Message}", startUrl, ex);
        }
        finally
        {
            viewModel.IsCrawling = false;
            _performanceTracker.LogMetrics();
            _logger.LogInformation("Crawl complete for {Url}, processed {Count} pages",
                startUrl, crawlResults.Count);
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
    /// Process the links found on a crawled page.
    /// </summary>
    /// <param name="result">The crawl result containing the found links.</param>
    /// <param name="depth">The current depth in the crawl hierarchy.</param>
    /// <param name="crawledURLs">Set of URLs that have already been crawled.</param>
    /// <param name="crawlResults">Dictionary of crawl results.</param>
    /// <param name="linksToCrawl">Queue of links to crawl.</param>
    /// <param name="options">Crawler options.</param>
    /// <param name="ct">Cancellation token to stop the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessFoundLinks(
        CrawlResult result,
        int depth,
        HashSet<string> crawledURLs,
        ConcurrentDictionary<string, CrawlResult> crawlResults,
        ConcurrentQueue<(string Url, int Depth, int Priority)> linksToCrawl,
        CrawlerOptions options,
        CancellationToken ct)
    {
        // Use a batch approach to process links
        var batch = new List<string>();
        var batchSize = 100; // Process links in batches of 100

        foreach (var foundUrl in result.CrawlLinks)
        {
            batch.Add(foundUrl);

            if (batch.Count >= batchSize)
            {
                await ProcessLinkBatch(batch, depth, crawledURLs, crawlResults, linksToCrawl, options, ct).ConfigureAwait(false);
                batch.Clear();
            }
        }

        // Process any remaining links
        if (batch.Count > 0)
        {
            await ProcessLinkBatch(batch, depth, crawledURLs, crawlResults, linksToCrawl, options, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Process a batch of links found on a crawled page.
    /// </summary>
    /// <param name="batch">Batch of URLs to process.</param>
    /// <param name="depth">The current depth in the crawl hierarchy.</param>
    /// <param name="crawledURLs">Set of URLs that have already been crawled.</param>
    /// <param name="crawlResults">Dictionary of crawl results.</param>
    /// <param name="linksToCrawl">Queue of links to crawl.</param>
    /// <param name="options">Crawler options.</param>
    /// <param name="ct">Cancellation token to stop the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessLinkBatch(
        List<string> batch,
        int depth,
        HashSet<string> crawledURLs,
        ConcurrentDictionary<string, CrawlResult> crawlResults,
        ConcurrentQueue<(string Url, int Depth, int Priority)> linksToCrawl,
        CrawlerOptions options,
        CancellationToken ct)
    {
        await Task.Run(() =>
        {
            foreach (var foundUrl in batch)
            {
                try
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

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

                    // Calculate priority based on URL characteristics
                    int priority = CalculateUrlPriority(normalizedUrl, depth);

                    // Add to queue with priority
                    linksToCrawl.Enqueue((normalizedUrl, depth + 1, priority));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing link: {Link}", foundUrl);
                }
            }
        }, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Calculate a priority score for a URL based on various heuristics.
    /// </summary>
    /// <param name="url">The URL to evaluate.</param>
    /// <param name="depth">The current depth in the crawl hierarchy.</param>
    /// <returns>A priority score (lower is higher priority).</returns>
    private static int CalculateUrlPriority(string url, int depth)
    {
        // Base priority is depth + 1 (depth 1 = priority 2)
        int priority = depth + 1;

        // Higher priority for URLs containing certain keywords
        if (url.Contains("index", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("home", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("main", StringComparison.OrdinalIgnoreCase))
        {
            priority -= 1;
        }

        // Lower priority for URLs with query parameters (likely dynamic content)
        if (url.Contains('?'))
        {
            priority += 1;
        }

        // Even lower priority for URLs with fragments
        if (url.Contains('#'))
        {
            priority += 2;
        }

        // Clamp priority between 1 and 10
        return Math.Max(1, Math.Min(10, priority));
    }

    /// <summary>
    /// Log the progress of the crawl operation.
    /// </summary>
    /// <param name="crawledCount">Number of pages crawled so far.</param>
    /// <param name="queueCount">Number of URLs in the queue.</param>
    /// <param name="currentDepth">Current depth being processed.</param>
    /// <param name="depthStats">Statistics of pages crawled at each depth.</param>
    private void LogProgress(int crawledCount, int queueCount, int currentDepth, ConcurrentDictionary<int, int> depthStats)
    {
        var depthSummary = string.Join(", ", depthStats.OrderBy(kv => kv.Key)
            .Select(kv => $"D{kv.Key}:{kv.Value}"));

        _logger.LogInformation("Crawl progress: {CrawledCount} crawled, {QueueCount} queued, Current depth: {Depth}, Depth stats: {DepthStats}",
            crawledCount, queueCount, currentDepth, depthSummary);
    }

    /// <summary>
    /// Generates a sitemap XML string from the given URLs.
    /// </summary>
    /// <param name="urls">The collection of URLs to include in the sitemap.</param>
    /// <returns>An XML string representing the sitemap.</returns>
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
        catch (Exception ex)
        {
            Debug.WriteLine($"Error generating sitemap XML: {ex.Message}");
            return string.Empty;
        }
    }
}
