using HtmlAgilityPack;
using WebSpark.HttpClientUtility.RequestResult;

namespace HttpClientCrawler.Crawler;

/// <summary>
/// Represents the result of a crawl operation for a single URL
/// </summary>
public class CrawlResult : HttpRequestResult<string>
{
    private readonly List<string> _responseLinks = [];

    /// <summary>
    /// List of errors encountered during crawling
    /// </summary>
    public List<string> Errors { get; } = [];

    /// <summary>
    /// The URL that was found during crawling
    /// </summary>
    public string FoundUrl { get; set; } = string.Empty;

    /// <summary>
    /// The depth of this crawl result in the crawl tree (1 = initial page)
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Initializes a new instance of the CrawlResult class
    /// </summary>
    public CrawlResult() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the CrawlResult class with values from an existing HttpRequestResult
    /// </summary>
    /// <param name="crawlResponse">The HttpRequestResult to copy values from</param>
    public CrawlResult(HttpRequestResult<string> crawlResponse) : base(crawlResponse)
    {
        if (crawlResponse == null)
        {
            throw new ArgumentNullException(nameof(crawlResponse));
        }

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

    /// <summary>
    /// Initializes a new instance of the CrawlResult class with specified parameters
    /// </summary>
    /// <param name="requestPath">The path of the request</param>
    /// <param name="foundUrl">The URL that was found</param>
    /// <param name="depth">The depth in the crawl tree</param>
    /// <param name="id">The unique identifier</param>
    public CrawlResult(string requestPath, string foundUrl, int depth, int id)
    {
        RequestPath = requestPath ?? throw new ArgumentNullException(nameof(requestPath));
        FoundUrl = foundUrl ?? throw new ArgumentNullException(nameof(foundUrl));
        Depth = depth;
        Id = id;
    }

    /// <summary>
    /// Gets a list of links extracted from the response HTML
    /// </summary>
    public List<string> CrawlLinks
    {
        get
        {
            try
            {
                _responseLinks.Clear();
                if (ResponseHtmlDocument != null)
                {
                    foreach (var link in ResponseHtmlDocument.DocumentNode
                        .Descendants("a")
                        .Select(a => SiteCrawlerHelpers.RemoveQueryAndOnPageLinks(
                            a.GetAttributeValue("href", string.Empty), RequestPath))
                        .Where(link => !string.IsNullOrWhiteSpace(link)))
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
            catch (Exception)
            {
                // Return empty list if there's an error parsing links
                return [];
            }
        }
    }

    /// <summary>
    /// Gets the parsed HTML document from the response results
    /// </summary>
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

    /// <summary>
    /// Returns a string representation of this crawl result
    /// </summary>
    /// <returns>A string containing the ID, depth, status code, and request path</returns>
    public override string ToString()
    {
        return $"ID:{Id} Depth:{Depth} Status:{StatusCode} URL:{RequestPath}";
    }
}
