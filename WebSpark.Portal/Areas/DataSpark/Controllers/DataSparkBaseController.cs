using System.Diagnostics;

namespace WebSpark.Portal.Areas.DataSpark.Controllers;

[Area("DataSpark")]
public abstract class DataSparkBaseController<T>(ILogger<T> logger) : Controller, ILogger where T : DataSparkBaseController<T>
{
   
    // Example method to log information
    protected void LogInformation(string message)
    {
        logger.LogInformation("LogInformation: {Message}", message);
    }

    // Example method to log errors
    protected void LogError(string message, Exception ex)
    {
        logger.LogError(ex, "LogError: {Message}", message);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return logger.BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logger.IsEnabled(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        logger.Log(logLevel, eventId, state, exception, formatter);
    }
}
