namespace HttpClientCrawler.Crawler;

/// <summary>
/// Configuration options for the web crawler.
/// </summary>
/// <remarks>
/// This class contains settings that control the crawling behavior, including limits,
/// timeouts, and output configurations. These settings can be customized to balance
/// performance, resource usage, and compliance with target site requirements.
/// </remarks>
public class CrawlerOptions
{
    /// <summary>
    /// Maximum number of pages to crawl.
    /// </summary>
    /// <remarks>
    /// This setting helps limit resource usage for large sites.
    /// For unbounded crawling (not recommended), set to Int32.MaxValue.
    /// </remarks>
    public int MaxPages { get; set; } = 500;

    /// <summary>
    /// Maximum depth to crawl from the start page.
    /// </summary>
    /// <remarks>
    /// Controls how far from the starting URL the crawler will follow links.
    /// Depth of 1 means only the start page, 2 includes pages linked from the start page, etc.
    /// </remarks>
    public int MaxDepth { get; set; } = 3;

    /// <summary>
    /// Delay between requests in milliseconds to avoid overloading the server.
    /// </summary>
    /// <remarks>
    /// Respects server resources by adding a delay between requests.
    /// A value of 0 means no delay (not recommended for production use).
    /// </remarks>
    public int RequestDelayMs { get; set; } = 200;

    /// <summary>
    /// User agent string to use for requests.
    /// </summary>
    /// <remarks>
    /// Identifies your crawler to web servers. Using a descriptive user agent
    /// with contact information is considered good practice.
    /// </remarks>
    public string UserAgent { get; set; } = "HttpClientCrawler/1.0";

    /// <summary>
    /// Whether to respect robots.txt rules.
    /// </summary>
    /// <remarks>
    /// When enabled, the crawler will check robots.txt files to determine
    /// if a URL is allowed to be crawled. Disabling this may violate site policies.
    /// </remarks>
    public bool RespectRobotsTxt { get; set; } = true;

    /// <summary>
    /// Whether to save crawled pages to disk.
    /// </summary>
    /// <remarks>
    /// When enabled, HTML content from crawled pages will be saved to the specified
    /// output directory. Useful for offline analysis or content archiving.
    /// </remarks>
    public bool SavePagesToDisk { get; set; } = false;

    /// <summary>
    /// Directory where crawled pages will be saved if SavePagesToDisk is true.
    /// </summary>
    /// <remarks>
    /// If null or empty, pages will be saved to a "pages" subdirectory in the application's base directory.
    /// The directory will be created if it doesn't exist.
    /// </remarks>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// Whether to validate HTML content of crawled pages.
    /// </summary>
    /// <remarks>
    /// When enabled, the crawler will perform basic HTML validation checks
    /// and log issues found. Useful for identifying problems with pages.
    /// </remarks>
    public bool ValidateHtml { get; set; } = false;

    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    /// <remarks>
    /// Maximum time to wait for a response from the server before considering the request failed.
    /// Default is 30 seconds, but can be adjusted based on network conditions and server response times.
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to use adaptive rate limiting based on server response.
    /// </summary>
    /// <remarks>
    /// When enabled, the crawler will dynamically adjust request delays based on server response times
    /// and error rates. This helps avoid overloading servers and getting temporarily blocked.
    /// </remarks>
    public bool UseAdaptiveRateLimiting { get; set; } = true;

    /// <summary>
    /// Maximum number of concurrent requests to make.
    /// </summary>
    /// <remarks>
    /// Controls parallelism for improved performance. Set to 1 for sequential crawling.
    /// Higher values increase throughput but also server load. Default balances performance and politeness.
    /// </remarks>
    public int MaxConcurrentRequests { get; set; } = 3;

    /// <summary>
    /// Whether to follow external links (links to different domains).
    /// </summary>
    /// <remarks>
    /// When false, only URLs on the same domain as the start URL will be crawled.
    /// Useful for limiting the scope of the crawl to a single site.
    /// </remarks>
    public bool FollowExternalLinks { get; set; } = false;

    /// <summary>
    /// Custom URL patterns to include (regex format).
    /// </summary>
    /// <remarks>
    /// If specified, only URLs matching these patterns will be crawled.
    /// Useful for targeting specific sections of a site or file types.
    /// </remarks>
    public List<string>? IncludePatterns { get; set; }

    /// <summary>
    /// Custom URL patterns to exclude (regex format).
    /// </summary>
    /// <remarks>
    /// URLs matching these patterns will be skipped during crawling.
    /// Useful for avoiding certain paths, file types, or sections of a site.
    /// </remarks>
    public List<string>? ExcludePatterns { get; set; }

    /// <summary>
    /// Whether to enable memory optimization for large crawls.
    /// </summary>
    /// <remarks>
    /// When enabled, the crawler will use more aggressive memory management techniques
    /// for large crawls, potentially sacrificing some performance for reduced memory usage.
    /// </remarks>
    public bool OptimizeMemoryUsage { get; set; } = false;

    /// <summary>
    /// Whether to generate a sitemap from crawled URLs.
    /// </summary>
    /// <remarks>
    /// When enabled, the crawler will generate a sitemap.xml file from successfully crawled URLs.
    /// </remarks>
    public bool GenerateSitemap { get; set; } = true;

    /// <summary>
    /// File path where the sitemap will be saved if GenerateSitemap is true.
    /// </summary>
    /// <remarks>
    /// If null or empty, the sitemap will be included in the results but not saved to disk.
    /// </remarks>
    public string? SitemapOutputPath { get; set; }
}