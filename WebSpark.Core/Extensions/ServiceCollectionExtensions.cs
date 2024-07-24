using Microsoft.Extensions.DependencyInjection;
using WebSpark.Core.Data;
using WebSpark.Core.Providers;

namespace WebSpark.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlogDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("ControlSpark");
        var conn = section.GetValue<string>("ConnString");

        if (section.GetValue<string>("DbProvider") == "SQLite")
            services.AddDbContext<WebSparkDbContext>(o => o.UseSqlite(conn));

        services.AddDatabaseDeveloperPageExceptionFilter();
        return services;
    }

    public static IServiceCollection AddBlogProviders(this IServiceCollection services)
    {
        services.AddScoped<IAuthorProvider, AuthorProvider>();
        services.AddScoped<Interfaces.IBlogProvider, BlogProvider>();
        services.AddScoped<IPostProvider, PostProvider>();
        services.AddScoped<IStorageProvider, StorageProvider>();
        services.AddScoped<IFeedProvider, FeedProvider>();
        services.AddScoped<Interfaces.ICategoryProvider, CategoryProvider>();
        services.AddScoped<IAnalyticsProvider, AnalyticsProvider>();
        services.AddScoped<INewsletterProvider, NewsletterProvider>();
        services.AddScoped<IEmailProvider, MailKitProvider>();
        services.AddScoped<IThemeProvider, ThemeProvider>();
        services.AddScoped<ISyndicationProvider, SyndicationProvider>();
        services.AddScoped<IAboutProvider, AboutProvider>();

        return services;
    }
}
