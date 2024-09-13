using HtmlAgilityPack;
using HttpClientUtility.SendService;

namespace HttpClientCrawler.Crawler;

public class CrawlResult : HttpClientSendRequest<string>
{
    public List<string> Errors { get; } = [];

    public CrawlResult() : base()
    {
    }

    public CrawlResult(HttpClientSendRequest<string> statusCall) : base(statusCall)
    {
    }
    private readonly List<string> _responseLinks = [];

    public CrawlResult(string requestPath, string foundUrl, int depth, int id)
    {
        RequestPath = requestPath;
        Depth = depth;
        FoundUrl = foundUrl;
        Id = id;

    }
    public string FoundUrl { get; set; } = string.Empty;
    public int Depth { get; set; }
    public List<string> CrawlLinks
    {
        get
        {
            _responseLinks.Clear();
            if (ResponseHtmlDocument != null)
            {
                foreach (var link in ResponseHtmlDocument.DocumentNode
                    .Descendants("a")
                    .Select(a => SiteCrawlerHelpers.RemoveQueryAndOnPageLinks(a.GetAttributeValue("href", null), RequestPath))
                    .Where(link => !string.IsNullOrWhiteSpace(link))
                    )
                {
                    if (_responseLinks.Contains(link))
                    {
                        continue;
                    }
                    if (SiteCrawlerHelpers.IsValidLink(link))
                    {
                        if (SiteCrawlerHelpers.IsSameDomain(link, RequestPath))
                        {
                            _responseLinks.Add(link);
                        }
                    }
                }
            }
            return _responseLinks;
        }
    }

    public HtmlDocument? ResponseHtmlDocument
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ResponseResults))
            {
                return null;
            }
            try
            {
                HtmlDocument htmlDoc = new();
                htmlDoc.LoadHtml(ResponseResults);
                return htmlDoc;
            }
            catch
            {
                return null;
            }
        }
    }
}
