namespace HttpClientCrawler.Crawler;

/// <summary>
/// Configuration options for the web crawler
/// </summary>
public class CrawlerOptions
{
    /// <summary>
    /// Maximum number of pages to crawl
    /// </summary>
    public int MaxPages { get; set; } = 500;

    /// <summary>
    /// Maximum depth to crawl from the start page
    /// </summary>
    public int MaxDepth { get; set; } = 3;

    /// <summary>
    /// Delay between requests in milliseconds to avoid overloading the server
    /// </summary>
    public int RequestDelayMs { get; set; } = 200;

    /// <summary>
    /// User agent string to use for requests
    /// </summary>
    public string UserAgent { get; set; } = "HttpClientCrawler/1.0";

    /// <summary>
    /// Whether to respect robots.txt rules
    /// </summary>
    public bool RespectRobotsTxt { get; set; } = true;

    /// <summary>
    /// Whether to save crawled pages to disk
    /// </summary>
    public bool SavePagesToDisk { get; set; } = false;

    /// <summary>
    /// Directory where crawled pages will be saved if SavePagesToDisk is true
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// Whether to validate HTML content of crawled pages
    /// </summary>
    public bool ValidateHtml { get; set; } = false;
}