using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Extensions.Logging;

namespace WebSpark.Core.Infrastructure.Logging
{
    public static class LoggingUtility
    {
        public static void ConfigureLogging(WebApplicationBuilder builder, string ApplicationName)
        {
            string logPath = $"C:\\websites\\WebSpark\\logs\\{ApplicationName}-log-.txt";
            builder.Logging.ClearProviders();
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
            builder.Logging.AddProvider(new SerilogLoggerProvider(Log.Logger));
            Log.Information("Logger setup complete. This is a test log entry.");
        }
    }
}
