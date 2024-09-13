namespace HttpClientCrawler.Crawler;

public interface ISiteCrawler
{
    Task<CrawlDomainViewModel> CrawlAsync(int maxCrawlDepth, string StartPath, CancellationToken ct = default);
}
