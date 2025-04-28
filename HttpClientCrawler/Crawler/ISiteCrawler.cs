namespace HttpClientCrawler.Crawler;

/// <summary>
/// Interface for web crawlers with configurable options
/// </summary>
public interface ISiteCrawler
{
    /// <summary>
    /// Crawls a website starting from the given URL
    /// </summary>
    /// <param name="startUrl">The URL to start crawling from</param>
    /// <param name="options">Crawler configuration options</param>
    /// <param name="ct">Cancellation token to stop the crawling process</param>
    /// <returns>A view model containing the crawl results</returns>
    Task<CrawlDomainViewModel> CrawlAsync(string startUrl, CrawlerOptions options, CancellationToken ct = default);
}
