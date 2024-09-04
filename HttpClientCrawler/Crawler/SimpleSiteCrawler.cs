using HtmlAgilityPack;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Xml.Linq;

namespace HttpClientCrawler.Crawler;

public class SimpleSiteCrawler(IHttpClientFactory factory) : ISiteCrawler
{
    private static readonly HashSet<string> crawledURLs = [];
    private static readonly ConcurrentQueue<CrawlResult> crawlQueue = new();
    private static readonly object lockObj = new();
    private static readonly ConcurrentDictionary<string, CrawlResult> resultsDict = new();
    private static readonly ConcurrentBag<string> notInSitemapLinks = [];
    private static readonly int maxCrawlDepth = 3; // Configurable maximum depth

    public async Task InitializeDomainAsync(string domainUrl, CancellationToken ct = default)
    {
        var sitemapUrl = new Uri(new Uri(domainUrl), "sitemap.xml").ToString();
        try
        {
            var response = await factory.CreateClient("SimpleSiteCrawler").GetAsync(sitemapUrl, ct);
            if (response.IsSuccessStatusCode)
            {
                var sitemapContent = await response.Content.ReadAsStringAsync(ct);
                var sitemapLinks = ParseSitemap(sitemapContent);
                foreach (var link in sitemapLinks)
                {
                    var normalizedLink = NormalizeUrl(link);
                    if (!crawledURLs.Contains(normalizedLink) && !resultsDict.ContainsKey(normalizedLink))
                    {
                        crawlQueue.Enqueue(new CrawlResult(normalizedLink, domainUrl, 1, 1));
                    }
                }
                Console.WriteLine($"Sitemap found and {sitemapLinks.Count} links added to the crawl queue.");
            }
            else
            {
                Console.WriteLine("Sitemap not found or inaccessible.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing sitemap: {ex.Message}");
        }
    }

    private static List<string> ParseSitemap(string sitemapContent)
    {
        var links = new List<string>();
        try
        {
            var xdoc = XDocument.Parse(sitemapContent);
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            links = xdoc.Root.Elements(ns + "url")
                            .Elements(ns + "loc")
                            .Select(e => e.Value)
                            .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing sitemap: {ex.Message}");
        }
        return links;
    }

    private static bool AddCrawlResult(CrawlResult? result)
    {
        if (result is null)
        {
            return false;
        }
        lock (lockObj)
        {
            var normalizedUrl = NormalizeUrl(result.RequestPath);
            if (resultsDict.ContainsKey(normalizedUrl))
            {
                return false;
            }
            result.RequestPath = normalizedUrl;
            resultsDict[normalizedUrl] = result;
            SavePageFireAndForget(result);

            foreach (var foundUrl in result.CrawlLinks.ToArray())
            {
                var normalizedFoundUrl = NormalizeUrl(foundUrl);
                if (crawledURLs.Contains(normalizedFoundUrl))
                {
                    continue;
                }
                if (resultsDict.ContainsKey(normalizedFoundUrl))
                {
                    continue;
                }
                if (crawlQueue.Any(w => w.RequestPath == normalizedFoundUrl))
                {
                    continue;
                }

                notInSitemapLinks.Add(normalizedFoundUrl); // Track links not in sitemap

                var newCrawl = new CrawlResult(normalizedFoundUrl, result.RequestPath, result.Depth + 1, crawledURLs.Count + 1);
                crawlQueue.Enqueue(newCrawl);
            }
            Console.WriteLine($"ID:{result.Id} CRAWLED:{resultsDict.Count:D5} QUEUE:{crawlQueue.Count:D5}  DEPTH:{result.Depth} TIME:{result.ElapsedMilliseconds:0,000} +++ Added Result: {result.RequestPath}");
            return true;
        }
    }

    private static async Task DelayRequestAsync()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(200)); // Adjust delay as needed
    }

    private async Task<CrawlResult?> CrawlPage(CrawlResult? crawlResult, CancellationToken ct = default)
    {
        if (crawlResult is null)
        {
            return null;
        }
        await DelayRequestAsync(); // Add delay before each request
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var normalizedUrl = NormalizeUrl(crawlResult.RequestPath);
            crawledURLs.Add(normalizedUrl);
            var response = await factory.CreateClient("SimpleSiteCrawler").GetAsync(normalizedUrl, ct);
            crawlResult.StatusCode = response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                crawlResult.ResponseResults = await response.Content.ReadAsStringAsync(ct);
            }
        }
        catch (HttpRequestException ex)
        {
            crawlResult.StatusCode = ex.StatusCode ?? HttpStatusCode.InternalServerError;
            crawlResult.Errors.Add($"HTTP error: {ex.Message}");
        }
        catch (Exception ex)
        {
            crawlResult.StatusCode = HttpStatusCode.InternalServerError;
            crawlResult.Errors.Add($"General error: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            crawlResult.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            crawlResult.CompletionDate = DateTime.Now;
        }
        return crawlResult;
    }

    public async Task<ICollection<CrawlResult>> CrawlAsync(int maxCrawlDepth, string link, CancellationToken ct = default)
    {
        await InitializeDomainAsync(link, ct);

        var firstCrawlResult = await StartInitialCrawlAsync(link, ct);
        if (firstCrawlResult != null)
        {
            int id = 1;
            id = await CrawlFoundLinks(firstCrawlResult, id, ct);
            id = await CrawlAllFoundLinks(id, ct);
            id = await ProcessCrawlQueue(id, ct);
        }

        // Output links found but not in the sitemap
        Console.WriteLine("Links found but not in the sitemap:");
        foreach (var url in notInSitemapLinks)
        {
            Console.WriteLine(url);
        }

        return resultsDict.Values;
    }

    private async Task<CrawlResult?> StartInitialCrawlAsync(string link, CancellationToken ct)
    {
        var firstCrawl = new CrawlResult(link, link, 1, 1);
        var firstCrawlResult = await CrawlPage(firstCrawl, ct);
        if (firstCrawlResult != null)
        {
            AddCrawlResult(firstCrawlResult);
            Console.WriteLine($"--First CrawlAsync Completed--{firstCrawlResult.RequestPath} -- found {firstCrawlResult.CrawlLinks.Count} links");
        }
        return firstCrawlResult;
    }

    private async Task<int> ProcessCrawlQueue(int id, CancellationToken ct)
    {
        var tasks = new List<Task>();
        while (crawlQueue.TryDequeue(out CrawlResult? crawlNext))
        {
            if (crawlNext is null || crawledURLs.Contains(NormalizeUrl(crawlNext.RequestPath)))
            {
                continue;
            }

            crawlNext.Id = id++;
            tasks.Add(Task.Run(async () =>
            {
                var queueCrawlResult = await CrawlPage(crawlNext, ct);
                if (queueCrawlResult is not null)
                {
                    AddCrawlResult(queueCrawlResult);
                }
            }));
        }
        await Task.WhenAll(tasks);
        return id;
    }

    private async Task<int> CrawlAllFoundLinks(int id, CancellationToken ct)
    {
        foreach (var item in resultsDict.Values)
        {
            foreach (var childLink in item.CrawlLinks.ToArray())
            {
                var normalizedChildLink = NormalizeUrl(childLink);
                if (crawledURLs.Contains(normalizedChildLink))
                {
                    continue;
                }
                var childCrawl = new CrawlResult(normalizedChildLink, item.RequestPath, 3, id++);
                var childCrawlResult = await CrawlPage(childCrawl, ct);
                if (childCrawlResult is not null)
                {
                    AddCrawlResult(childCrawlResult);
                }
            }
        }
        return id;
    }

    private async Task<int> CrawlFoundLinks(CrawlResult? crawlResult, int id, CancellationToken ct)
    {
        if (crawlResult is null) { return id; }
        foreach (var childLink in crawlResult.CrawlLinks.ToArray())
        {
            var normalizedChildLink = NormalizeUrl(childLink);
            if (crawledURLs.Contains(normalizedChildLink)) { continue; }
            var childCrawl = new CrawlResult(normalizedChildLink, crawlResult.RequestPath, 2, id++);
            var childCrawlResult = await CrawlPage(childCrawl, ct);
            if (childCrawlResult is not null)
            {
                AddCrawlResult(childCrawlResult);
            }
        }
        return id;
    }

    private static string NormalizeUrl(string url)
    {
        var uri = new Uri(url);
        return uri.GetLeftPart(UriPartial.Path).TrimEnd('/') + uri.Query;
    }

    public static async Task SavePageAsync(CrawlResult result)
    {
        if (result == null || string.IsNullOrWhiteSpace(result.ResponseResults))
        {
            Console.WriteLine("No content to save or result is null.");
            return;
        }

        string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pages");
        Directory.CreateDirectory(directoryPath);

        string safeFileName = GetSafeFileName(result.RequestPath, directoryPath);
        string filePath = Path.Combine(directoryPath, safeFileName);

        string updatedHtmlContent = ResolveRelativeLinks(result.ResponseResults, result.RequestPath);

        var validationMessages = ValidateHtml(updatedHtmlContent);
        if (validationMessages != null)
        {
            Console.WriteLine($"Validation errors found for {result.RequestPath}:");
            foreach (var message in validationMessages)
            {
                Console.WriteLine(message);
            }
        }

        EnsureDirectoryExists(filePath);
        await File.WriteAllTextAsync(filePath, updatedHtmlContent);
        Console.WriteLine($"Saved {result.RequestPath} to {filePath}");
    }

    public static void EnsureDirectoryExists(string filePath)
    {
        try
        {
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ensuring directory exists: {ex.Message}");
        }
    }

    public static string GetSafeFileName(string url, string outputDir)
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

            path = path.Replace('/', '\\').TrimStart(Path.DirectorySeparatorChar);
            path = path.Length <= 150 ? path : path.Substring(0, 150);

            return Path.Combine(outputDir, path);
        }
        catch
        {
            return Path.Combine(outputDir, "default_safe_name.html");
        }
    }

    private static string ResolveRelativeLinks(string htmlContent, string baseUrl)
    {
        var baseUri = new Uri(baseUrl);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        var nodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]|//img[@src]|//link[@href]|//script[@src]|//iframe[@src]|//embed[@src]|//object[@data]|//source[@src]|//track[@src]|//form[@action]|//area[@href]|//blockquote[@cite]|//q[@cite]|//ins[@cite]|//del[@cite]");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                string attributeName = node.Name switch
                {
                    "a" or "link" or "area" or "blockquote" or "q" or "ins" or "del" => "href",
                    "form" => "action",
                    "object" => "data",
                    "track" or "source" => "src",
                    _ => "src"
                };

                string originalValue = node.Attributes[attributeName]?.Value;

                if (!string.IsNullOrEmpty(originalValue) && Uri.TryCreate(originalValue, UriKind.Relative, out Uri relativeUri))
                {
                    var absoluteUri = new Uri(baseUri, relativeUri);
                    node.Attributes[attributeName].Value = absoluteUri.AbsoluteUri;
                }
            }
        }
        return htmlDoc.DocumentNode.OuterHtml;
    }

    public static List<string>? ValidateHtml(string htmlContent)
    {
        List<string> messages = [];

        var htmlDoc = new HtmlDocument
        {
            OptionCheckSyntax = true
        };
        htmlDoc.LoadHtml(htmlContent);

        if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
        {
            messages.AddRange(htmlDoc.ParseErrors.Select(error => $"Line {error.Line}: {error.Reason}"));
        }

        var imgNodes = htmlDoc.DocumentNode.SelectNodes("//img[not(@alt)]");
        if (imgNodes != null)
        {
            messages.AddRange(imgNodes.Select(node => $"Image tag without alt attribute found. Line: {node.Line}"));
        }

        return messages.Count == 0 ? null : messages;
    }

    public static void SavePageFireAndForget(CrawlResult result)
    {
        Task.Run(async () =>
        {
            try
            {
                await SavePageAsync(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving the page: {ex.Message}");
            }
        }).ConfigureAwait(false);
    }
}
