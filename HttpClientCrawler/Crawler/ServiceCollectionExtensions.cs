using Microsoft.Extensions.DependencyInjection;

namespace HttpClientCrawler.Crawler;

/// <summary>
/// Extension methods for setting up HttpClientCrawler services in an IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds HttpClientCrawler services to the specified IServiceCollection
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    public static IServiceCollection AddHttpClientCrawler(this IServiceCollection services)
    {
        // Register SignalR hub
        services.AddSignalR();

        // Register the crawlers
        services.AddTransient<ISiteCrawler, SiteCrawler>();
        services.AddTransient<SimpleSiteCrawler>();

        // Return the IServiceCollection so that additional calls can be chained
        return services;
    }

    /// <summary>
    /// Adds HttpClientCrawler services with customized options to the specified IServiceCollection
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <param name="configureOptions">The action used to configure the crawler options</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    public static IServiceCollection AddHttpClientCrawler(
        this IServiceCollection services,
        Action<CrawlerOptions> configureOptions)
    {
        if (configureOptions == null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        // Create options
        var options = new CrawlerOptions();
        configureOptions(options);

        // Register options
        services.AddSingleton(options);

        // Add the services
        return AddHttpClientCrawler(services);
    }
}