using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Extensions.Logging;

namespace WebSpark.Core.Infrastructure.Logging;


public static class LoggingUtility
{
    public static void ConfigureLogging(WebApplicationBuilder builder, string ApplicationName)
    {
        string logPath = $"C:\\websites\\WebSpark\\logs\\{ApplicationName}-log-.txt";

        // Clear existing logging providers
        builder.Logging.ClearProviders();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Add Serilog to the logging providers
        builder.Logging.AddProvider(new SerilogLoggerProvider(Log.Logger));

        // Add filtering for EF Core logging
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

        // Log a test entry to confirm setup
        Log.Information("Logger setup complete. This is a test log entry.");
    }
}
