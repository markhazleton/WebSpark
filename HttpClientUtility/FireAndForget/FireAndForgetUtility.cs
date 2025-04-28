using HttpClientUtility.Utilities.Logging;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HttpClientUtility.FireAndForget;

/// <summary>
/// Utility class for safely executing fire-and-forget tasks with standardized exception handling and logging.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FireAndForgetUtility"/> class.
/// </remarks>
/// <param name="logger">The logger instance used for logging exceptions and task statuses.</param>
public sealed class FireAndForgetUtility(ILogger<FireAndForgetUtility> logger)
{
    private readonly ILogger<FireAndForgetUtility> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Safely executes a fire-and-forget task with standardized logging and exception handling.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="operationName">A descriptive name for the operation being performed.</param>
    /// <param name="ct">Optional cancellation token.</param>
    public void SafeFireAndForget(Task task, string operationName = "Unnamed Operation", CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        // Generate correlation ID for tracking this operation
        string correlationId = Guid.NewGuid().ToString();

        // Log the start of the operation
        _logger.LogDebug(
            "Starting fire-and-forget operation: {OperationName} [CorrelationId: {CorrelationId}]",
            operationName,
            correlationId);

        var stopwatch = Stopwatch.StartNew();

        task.ContinueWith(t =>
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (t.IsFaulted && t.Exception != null)
            {
                foreach (var ex in t.Exception.InnerExceptions)
                {
                    // Create context data for rich exception logging
                    var contextData = new Dictionary<string, object>
                    {
                        ["OperationName"] = operationName,
                        ["ElapsedMs"] = elapsedMs,
                        ["TaskStatus"] = t.Status.ToString()
                    };

                    // Use our standardized error logging utility
                    ErrorHandlingUtility.LogException(
                        ex,
                        _logger,
                        $"Fire-and-Forget {operationName}",
                        correlationId,
                        contextData);
                }
            }
            else if (t.IsCanceled || ct.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Fire-and-forget operation canceled after {ElapsedMs}ms: {OperationName} [CorrelationId: {CorrelationId}]",
                    elapsedMs,
                    operationName,
                    correlationId);
            }
            else
            {
                _logger.LogInformation(
                    "Fire-and-forget operation completed successfully in {ElapsedMs}ms: {OperationName} [CorrelationId: {CorrelationId}]",
                    elapsedMs,
                    operationName,
                    correlationId);
            }
        }, ct, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).ConfigureAwait(false);
    }

    /// <summary>
    /// Safely executes a fire-and-forget task with standardized logging and exception handling.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <param name="operationName">A descriptive name for the operation being performed.</param>
    /// <param name="ct">Optional cancellation token.</param>
    public void SafeFireAndForget<T>(Task<T> task, string operationName = "Unnamed Operation", CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        // Generate correlation ID for tracking this operation
        string correlationId = Guid.NewGuid().ToString();

        // Log the start of the operation
        _logger.LogDebug(
            "Starting fire-and-forget operation with result type {ResultType}: {OperationName} [CorrelationId: {CorrelationId}]",
            typeof(T).Name,
            operationName,
            correlationId);

        var stopwatch = Stopwatch.StartNew();

        task.ContinueWith(t =>
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (t.IsFaulted && t.Exception != null)
            {
                foreach (var ex in t.Exception.InnerExceptions)
                {
                    // Create context data for rich exception logging
                    var contextData = new Dictionary<string, object>
                    {
                        ["OperationName"] = operationName,
                        ["ElapsedMs"] = elapsedMs,
                        ["TaskStatus"] = t.Status.ToString(),
                        ["ResultType"] = typeof(T).Name
                    };

                    // Use our standardized error logging utility
                    ErrorHandlingUtility.LogException(
                        ex,
                        _logger,
                        $"Fire-and-Forget {operationName}",
                        correlationId,
                        contextData);
                }
            }
            else if (t.IsCanceled || ct.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Fire-and-forget operation canceled after {ElapsedMs}ms: {OperationName} [CorrelationId: {CorrelationId}]",
                    elapsedMs,
                    operationName,
                    correlationId);
            }
            else
            {
                try
                {
                    // Try to log the result type and success
                    var resultInfo = t.Result != null
                        ? $"with result of type {t.Result.GetType().Name}"
                        : "with null result";

                    _logger.LogInformation(
                        "Fire-and-forget operation completed successfully in {ElapsedMs}ms {ResultInfo}: {OperationName} [CorrelationId: {CorrelationId}]",
                        elapsedMs,
                        resultInfo,
                        operationName,
                        correlationId);
                }
                catch (Exception ex)
                {
                    // This would be unusual but handle just in case
                    _logger.LogWarning(
                        ex,
                        "Fire-and-forget operation completed but error accessing result in {ElapsedMs}ms: {OperationName} [CorrelationId: {CorrelationId}]",
                        elapsedMs,
                        operationName,
                        correlationId);
                }
            }
        }, ct, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).ConfigureAwait(false);
    }
}
