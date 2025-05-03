using Microsoft.Extensions.Options;
using WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

namespace WebSpark.Portal.Areas.GitHubSpark.Extensions;

/// <summary>
/// Extension methods for configuring GitHubSpark services in the DI container
/// </summary>
public static class GitHubSparkServiceExtensions
{
    /// <summary>
    /// Adds GitHubSpark services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddGitHubSparkServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure GitHubServiceOptions from appsettings.json or user secrets
        services.Configure<GitHubServiceOptions>(options =>
        {
            options.PersonalAccessToken = configuration["GitHubSpark:GitHubPAT"] ?? configuration["GitHubPAT"] ?? string.Empty;
            if (string.IsNullOrEmpty(options.PersonalAccessToken))
            {
                options.PersonalAccessToken = configuration["GitHubPAT"] ?? string.Empty;
            }


            options.UserAgent = configuration["GitHubSpark:GitHubUserAgent"] ?? configuration["GitHubUserAgent"] ?? "WebSparkGitHubClient";
            options.DefaultCacheDurationMinutes = configuration.GetValue<int>("GitHubSpark:GitHubCacheDurationMinutes",
                configuration.GetValue<int>("GitHubCacheDurationMinutes", 300));
            options.MaxRequestsPerHour = configuration.GetValue<int>("GitHubSpark:GitHubMaxRequestsPerHour",
                configuration.GetValue<int>("GitHubMaxRequestsPerHour", 50));
        });

        // Register the options directly for services that can't use IOptions
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<GitHubServiceOptions>>().Value);

        // Register GitHub services
        services.AddScoped<IGitHubUserService, GitHubUserService>();
        services.AddScoped<IGitHubRepositoryService, GitHubRepositoryService>();

        return services;
    }
}