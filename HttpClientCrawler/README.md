# HttpClientCrawler

HttpClientCrawler is a flexible and configurable web crawler library for .NET applications. It provides functionality to crawl websites, extract links, generate sitemaps, and save HTML content.

## Features

- Configurable crawling with customizable options
- Support for robots.txt parsing and adherence
- SignalR integration for real-time crawl progress updates
- HTML content validation and link extraction
- Sitemap generation
- Rate limiting to avoid overloading target servers
- Proper error handling and logging

## Getting Started

### Installation

Add a reference to the HttpClientCrawler library in your project:

```xml
<ItemGroup>
  <ProjectReference Include="..\path\to\HttpClientCrawler\HttpClientCrawler.csproj" />
</ItemGroup>
```

### Usage

#### Registering Services

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
    options.RespectRobotsTxt = true;
    options.UserAgent = "MyWebCrawler/1.0";
    options.SavePagesToDisk = true;
    options.OutputDirectory = "C:\\CrawlOutput";
});
```

#### Using the Crawler

Inject and use the `ISiteCrawler` or `SimpleSiteCrawler` in your application:

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

    public async Task CrawlWebsiteAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new CrawlerOptions
            {
                MaxPages = 500,
                MaxDepth = 3,
                RequestDelayMs = 200
            };
            
            var result = await _crawler.CrawlAsync(url, options, cancellationToken);
            
            _logger.LogInformation("Crawl completed. Processed {Count} pages", 
                result.CrawlResults.Count);
                
            // Process the results
            foreach (var crawlResult in result.CrawlResults)
            {
                // Access crawl data
                Console.WriteLine($"URL: {crawlResult.RequestPath}, Status: {crawlResult.StatusCode}");
            }
            
            // Access the generated sitemap
            Console.WriteLine(result.Sitemap);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during crawl operation");
        }
    }
}
```

### Real-time Updates

If you want to receive real-time updates during crawling, configure SignalR in your application:

```csharp
// In Program.cs or Startup.cs
app.MapHub<CrawlHub>("/crawlhub");

// In your JavaScript client
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/crawlhub")
    .build();
    
connection.on("UrlFound", (message) => {
    console.log(message);
    // Update UI with crawl progress
});

await connection.start();
```

## Best Practices

- Set reasonable limits for `MaxPages` and `MaxDepth` to avoid excessive crawling
- Always use a meaningful `UserAgent` string that identifies your crawler
- Respect robots.txt rules by setting `RespectRobotsTxt = true`
- Add a delay between requests to avoid overloading target servers
- Use proper exception handling and logging
- Consider implementing a caching mechanism for frequently crawled sites

## License

This project is licensed under the MIT License.
