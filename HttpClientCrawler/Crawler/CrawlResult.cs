using HtmlAgilityPack;
using HttpClientUtility.RequestResult;

namespace HttpClientCrawler.Crawler;

public class CrawlResult : HttpRequestResult<string>
{
    public List<string> Errors { get; } = [];

    public CrawlResult() : base()
    {
    }

    public CrawlResult(HttpRequestResult<string> crawlResponse) : base(crawlResponse)
    {
        ResponseResults = crawlResponse.ResponseResults;
        StatusCode = crawlResponse.StatusCode;
        Errors.AddRange(crawlResponse.ErrorList);
        RequestBody = crawlResponse.RequestBody;
        RequestHeaders = crawlResponse.RequestHeaders;
        RequestPath = crawlResponse.RequestPath;
        RequestMethod = crawlResponse.RequestMethod;
        Iteration = crawlResponse.Iteration;
        CacheDurationMinutes = crawlResponse.CacheDurationMinutes;
        Retries = crawlResponse.Retries;
        CompletionDate = crawlResponse.CompletionDate;
        ElapsedMilliseconds = crawlResponse.ElapsedMilliseconds;
        Id = crawlResponse.Id;
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
