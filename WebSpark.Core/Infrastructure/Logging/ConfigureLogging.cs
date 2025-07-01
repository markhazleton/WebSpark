using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace WebSpark.Core.Infrastructure.Logging;

public static class LoggingUtility
{
    public static void ConfigureLogging(WebApplicationBuilder builder, string applicationName)
    {
        // Get log path from configuration or use default
        string logPath = builder.Configuration.GetValue<string>("WebSpark:LogFilePath")
                         ?? $"C:\\websites\\WebSpark\\logs\\{applicationName}-log-.txt";

        // Clear existing logging providers
        builder.Logging.ClearProviders();

        // Enable Serilog self-log for troubleshooting in Development environment only
        if (builder.Environment.IsDevelopment())
        {
            Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"Serilog: {msg}"));
        }

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Information() // Set minimum level to Information
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();

        // Add Serilog to the logging providers
        builder.Logging.AddProvider(new SerilogLoggerProvider(Log.Logger));

        // Add filtering for EF Core logging
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

        // Log a test entry to confirm setup
        Log.Information("Logger setup complete. This is a test log entry.");
    }
}
