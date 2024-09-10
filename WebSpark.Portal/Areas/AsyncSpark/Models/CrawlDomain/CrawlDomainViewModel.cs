using HttpClientCrawler.Crawler;

namespace WebSpark.Portal.Areas.AsyncSpark.Models.CrawlDomain;

public class CrawlDomainViewModel
{
    public bool IsCrawling { get; set; }
    public string StartPath { get; set; }
    public int MaxPagesCrawled { get; set; } = 500; // Default maximum pages for crawling
    public ICollection<CrawlResult> CrawlResults { get; set; } = [];
}
