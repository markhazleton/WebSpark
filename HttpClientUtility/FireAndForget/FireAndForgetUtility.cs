using Microsoft.Extensions.Logging;

namespace HttpClientUtility.FireAndForget;

/// <summary>
/// Utility class for safely executing fire-and-forget tasks with exception handling and logging.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FireAndForgetUtility"/> class.
/// </remarks>
/// <param name="logger">The logger instance used for logging exceptions and task statuses.</param>
public sealed class FireAndForgetUtility(ILogger<FireAndForgetUtility> logger)
{
    private readonly ILogger<FireAndForgetUtility> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Safely executes a fire-and-forget task with logging and exception handling.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="ct">Optional cancellation token.</param>
    public void SafeFireAndForget(Task task, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        task.ContinueWith(t =>
        {
            if (t.IsFaulted && t.Exception != null)
            {
                foreach (var ex in t.Exception.InnerExceptions)
                {
                    _logger.LogError(ex, "Unhandled exception occurred: {ExceptionMessage}", ex.Message);
                }
            }
            else if (t.IsCanceled || ct.IsCancellationRequested)
            {
                _logger.LogInformation("Task was canceled.");
            }
            else
            {
                _logger.LogInformation("Task completed successfully.");
            }
        }, ct, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).ConfigureAwait(false);
    }
}
