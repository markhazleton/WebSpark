namespace HttpClientCrawler.Crawler;

public interface ISiteCrawler
{
    Task<ICollection<CrawlResult>> CrawlAsync(int maxCrawlDepth, string StartPath, CancellationToken ct = default);
}
