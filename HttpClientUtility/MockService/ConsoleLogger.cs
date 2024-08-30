using Microsoft.Extensions.Logging;

namespace HttpClientUtility.MockService;

/// <summary>
/// Represents a console logger implementation.
/// </summary>
public class ConsoleLogger : ICommonLogger, ILogger
{
    /// <inheritdoc/>
    IDisposable? ILogger.BeginScope<TState>(TState state)
    {
        return NoOpDisposable.Instance;
    }

    private class NoOpDisposable : IDisposable
    {
        public static NoOpDisposable Instance { get; } = new NoOpDisposable();

        private NoOpDisposable() { }

        public void Dispose()
        {
            // No operation
        }
    }
    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        // You can filter log levels here if needed
        return true;
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);
        if (!string.IsNullOrEmpty(message))
        {
            Console.WriteLine($"{DateTime.Now} [{logLevel}] {message}");
            if (exception != null)
            {
                Console.WriteLine(exception);
            }
        }
    }

    /// <summary>
    /// Tracks an event.
    /// </summary>
    /// <param name="message">The message to track.</param>
    public void TrackEvent(string message)
    {
        Console.WriteLine($"{DateTime.Now} [TrackEvent] {message}");
    }

    /// <summary>
    /// Tracks an exception.
    /// </summary>
    /// <param name="exception">The exception to track.</param>
    /// <param name="message">The message to track.</param>
    public void TrackException(Exception exception, string message)
    {
        Console.WriteLine($"{DateTime.Now} [TrackException] {message}\n {exception.Message}");
    }
}

