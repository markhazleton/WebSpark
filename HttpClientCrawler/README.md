# HttpClientCrawler

HttpClientCrawler is a flexible, high-performance web crawler library for .NET applications. It provides robust functionality to crawl websites, extract links, generate sitemaps, and save HTML content with configurable performance optimizations and polite crawling features.

## Features

- **Configurable Crawling**: Extensive options to customize crawling behavior
- **Performance Optimizations**: Memory usage tracking, adaptive rate limiting, and parallel processing
- **Robots.txt Compliance**: Respects robots.txt directives to ensure ethical crawling
- **Sitemap Generation**: Automatically generates sitemap.xml from crawled URLs
- **HTML Validation**: Optional validation of HTML content with detailed reporting
- **Content Storage**: Save crawled HTML content to disk with relative link resolution
- **Real-time Updates**: SignalR integration for crawl progress reporting
- **Detailed Logging**: Structured logging with performance metrics and error tracking
- **Thread Safety**: Concurrent operations with proper resource management

## Getting Started

### Installation

Add the HttpClientCrawler package to your project:

```bash
dotnet add package HttpClientCrawler
```

### Basic Usage

Register the HttpClientCrawler services in your application's dependency injection container:

```csharp
// In Program.cs or Startup.cs
using HttpClientCrawler.Crawler;

// Register with default options
services.AddHttpClientCrawler();

// Or with custom options
services.AddHttpClientCrawler(options =>
{
    options.MaxPages = 1000;
    options.MaxDepth = 5;
    options.RequestDelayMs = 300;
    options.UserAgent = "MyWebCrawler/1.0 (+https://example.com/bot)";
    options.RespectRobotsTxt = true;
    options.UseAdaptiveRateLimiting = true;
    options.MaxConcurrentRequests = 4;
});
```

### Using the Crawler

Inject and use the `ISiteCrawler` in your application:

```csharp
public class CrawlerService
{
    private readonly ISiteCrawler _crawler;
    private readonly ILogger<CrawlerService> _logger;

    public CrawlerService(ISiteCrawler crawler, ILogger<CrawlerService> logger)
    {
        _crawler = crawler;
        _logger = logger;
    }

    public async Task<CrawlDomainViewModel> CrawlWebsiteAsync(
        string url, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new CrawlerOptions
            {
                // Basic configuration
                MaxPages = 500,
                MaxDepth = 3,
                RequestDelayMs = 200,
                
                // Performance settings
                TimeoutSeconds = 30,
                UseAdaptiveRateLimiting = true,
                MaxConcurrentRequests = 3,
                OptimizeMemoryUsage = true,
                
                // Content handling
                ValidateHtml = true,
                SavePagesToDisk = true,
                OutputDirectory = "C:\\CrawlOutput",
                
                // URL filtering
                FollowExternalLinks = false,
                ExcludePatterns = new List<string> { "\\.pdf$", "\\.zip$", "\\?sort=" }
            };
            
            var result = await _crawler.CrawlAsync(url, options, cancellationToken);
            
            _logger.LogInformation("Crawl completed. Processed {Count} pages", 
                result.CrawlResults.Count);
                
            // Process the results
            foreach (var crawlResult in result.CrawlResults)
            {
                Console.WriteLine($"URL: {crawlResult.RequestPath}, Status: {crawlResult.StatusCode}");
                
                if (crawlResult.Errors.Any())
                {
                    Console.WriteLine($"Errors: {string.Join(", ", crawlResult.Errors)}");
                }
            }
            
            // Access the generated sitemap
            if (!string.IsNullOrEmpty(result.Sitemap))
            {
                await File.WriteAllTextAsync("sitemap.xml", result.Sitemap);
                Console.WriteLine("Sitemap generated and saved to sitemap.xml");
            }
            
            return result;
        }
        catch (CrawlException ex)
        {
            _logger.LogError(ex, "Crawl error for URL {Url}: {StatusCode}", 
                ex.Url, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during crawl operation");
            throw;
        }
    }
}
```

### Exception Handling

The library provides specialized exception types for better error handling:

```csharp
try
{
    var result = await _crawler.CrawlAsync(url, options);
    // Process result...
}
catch (CrawlException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    // Handle 404 errors
}
catch (CrawlException ex)
{
    // Handle other crawl-specific errors with context
    Console.WriteLine($"URL: {ex.Url}, Status: {ex.StatusCode}, Depth: {ex.Depth}");
}
catch (Exception ex)
{
    // Handle unexpected errors
}
```

### Performance Tracking

The crawler automatically tracks performance metrics which are available in the logs:

```
info: HttpClientCrawler.Crawler.SimpleSiteCrawler[0]
      Crawl a3c8e7fb: Total execution time: 15.23 seconds
info: HttpClientCrawler.Crawler.SimpleSiteCrawler[0]
      Crawl a3c8e7fb: Operation 'PageCrawl': 152 calls, 12450.32 ms total, 81.91 ms average
info: HttpClientCrawler.Crawler.SimpleSiteCrawler[0]
      Crawl a3c8e7fb: Operation 'RobotsTxtProcessing': 1 calls, 230.15 ms total, 230.15 ms average
```

### Real-time Updates

For real-time crawl progress, configure SignalR in your application:

```csharp
// In Program.cs or Startup.cs
app.MapHub<CrawlHub>("/crawlhub");

// In your JavaScript client
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/crawlhub")
    .build();
    
connection.on("UrlFound", (message) => {
    console.log(`Found URL: ${message.url} (Status: ${message.statusCode})`);
    // Update UI with crawl progress
});

connection.on("CrawlProgress", (progress) => {
    console.log(`Crawl progress: ${progress.processedCount}/${progress.totalCount}`);
    // Update progress bar
});

await connection.start();
```

## Advanced Usage

### URL Prioritization

The crawler automatically prioritizes URLs based on various heuristics:

```csharp
// Configure custom URL prioritization
options.IncludePatterns = new List<string> {
    "^/products/.*",     // Process product pages first
    "^/categories/.*",   // Process category pages next
};
options.ExcludePatterns = new List<string> {
    "^/account/.*",      // Skip account pages
    "\\.(pdf|zip|exe)$", // Skip binary files
    "\\?sort=",          // Skip sorting variations
};
```

### Adaptive Rate Limiting

The crawler can automatically adjust request delays based on server response:

```csharp
// Configure adaptive rate limiting
options.UseAdaptiveRateLimiting = true;
options.RequestDelayMs = 200;  // Initial delay, will be adjusted automatically
```

### Memory Optimization

For large crawl operations, enable memory optimization:

```csharp
// Configure memory optimization for large crawls
options.OptimizeMemoryUsage = true;
options.MaxPages = 10000;
```

## Best Practices

- **Set Appropriate Limits**: Configure `MaxPages` and `MaxDepth` based on your needs
- **Be Respectful**: Use a descriptive `UserAgent` with contact information
- **Respect Website Policies**: Keep `RespectRobotsTxt = true` and implement appropriate delays
- **Monitor Performance**: Use the performance metrics to identify bottlenecks
- **Handle Errors Gracefully**: Implement proper exception handling with `CrawlException`
- **Implement Rate Limiting**: Use `RequestDelayMs` and `UseAdaptiveRateLimiting` to be polite
- **Filter Content**: Use `IncludePatterns` and `ExcludePatterns` to focus on relevant content
- **Optimize for Large Crawls**: Enable `OptimizeMemoryUsage` for crawling large sites

## Performance Considerations

- Increase `MaxConcurrentRequests` for faster crawling (be mindful of server impact)
- Decrease `TimeoutSeconds` for unresponsive servers
- Use `OptimizeMemoryUsage` for large crawls (>1000 pages)
- Balance crawl speed with server impact using `RequestDelayMs`

## License

This project is licensed under the MIT License.
