using Microsoft.Extensions.Logging;
using System.Net;
using System.Runtime.ExceptionServices;

namespace HttpClientUtility.Utilities.Logging;

/// <summary>
/// Provides centralized error handling utilities for consistent exception management across the library.
/// </summary>
public static class ErrorHandlingUtility
{
    /// <summary>
    /// Enriches an exception with additional context information to aid in troubleshooting.
    /// </summary>
    /// <param name="exception">The original exception.</param>
    /// <param name="contextMessage">Additional context message to include.</param>
    /// <param name="contextData">Dictionary containing relevant context data.</param>
    /// <returns>A new exception wrapping the original one, enriched with context information.</returns>
    public static Exception EnrichException(Exception exception, string contextMessage, Dictionary<string, object>? contextData = null)
    {
        // Create a descriptive message combining original exception and context
        var enrichedMessage = $"{contextMessage}: {exception.Message}";

        // Include context data in the message if provided
        if (contextData != null && contextData.Count > 0)
        {
            enrichedMessage += $" | Context: {string.Join(", ", contextData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
        }

        // Create an appropriate exception type based on the original exception
        Exception enrichedException = exception switch
        {
            HttpRequestException httpEx => new HttpRequestException(
                enrichedMessage,
                httpEx.InnerException,
                httpEx.StatusCode),

            OperationCanceledException opEx => new OperationCanceledException(
                enrichedMessage,
                opEx.InnerException,
                opEx.CancellationToken),

            ArgumentException argEx => new ArgumentException(
                enrichedMessage,
                argEx.ParamName,
                argEx.InnerException),

            // Default case: wrap in a more general exception while preserving the stack trace
            _ => WrapWithStackPreservation(exception, enrichedMessage)
        };

        return enrichedException;
    }

    /// <summary>
    /// Wraps an exception while preserving its original stack trace.
    /// </summary>
    /// <param name="exception">The exception to wrap.</param>
    /// <param name="message">The new message for the wrapper exception.</param>
    /// <returns>A new exception with the preserved stack trace.</returns>
    private static Exception WrapWithStackPreservation(Exception exception, string message)
    {
        try
        {
            // Capture and preserve the original stack trace
            ExceptionDispatchInfo.Capture(exception).Throw();
            return exception; // This line will never execute, but compiler requires it
        }
        catch (Exception caughtEx)
        {
            // Create a new exception with the enriched message, preserving the original as inner
            return new Exception(message, caughtEx);
        }
    }

    /// <summary>
    /// Creates an appropriate HTTP response content object for an exception.
    /// </summary>
    /// <typeparam name="T">The type of the response content.</typeparam>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="logger">The logger to use for logging the exception.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <param name="operationName">The name of the operation that failed.</param>
    /// <returns>An HTTP response content object with appropriate error information.</returns>
    public static HttpResponseContent<T> CreateErrorResponse<T>(
        Exception exception,
        ILogger logger,
        string correlationId,
        string operationName)
    {
        // Log the exception with appropriate level and context
        var contextData = new Dictionary<string, object>
        {
            ["OperationName"] = operationName,
            ["CorrelationId"] = correlationId,
            ["ExceptionType"] = exception.GetType().Name
        };

        // Determine the appropriate status code based on the exception
        var statusCode = DetermineStatusCodeForException(exception);

        // Determine log level
        LogLevel logLevel;

        if (exception is OperationCanceledException || exception is TaskCanceledException)
        {
            logLevel = LogLevel.Warning;
        }
        else if (exception is HttpRequestException httpEx && httpEx.StatusCode.HasValue && (int)httpEx.StatusCode.Value < 500)
        {
            logLevel = LogLevel.Warning;
        }
        else
        {
            logLevel = LogLevel.Error;
        }

        logger.Log(
            logLevel,
            exception,
            "Error during {OperationName}: {ErrorMessage} [CorrelationId: {CorrelationId}]",
            operationName,
            exception.Message,
            correlationId);

        // Create appropriate error message for the response
        string errorMessage = FormatErrorMessageForResponse(exception, operationName, correlationId);

        // Create and return the error response
        return HttpResponseContent<T>.Failure(errorMessage, statusCode);
    }

    /// <summary>
    /// Formats an error message for inclusion in a response.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="operationName">The name of the operation that failed.</param>
    /// <param name="correlationId">The correlation ID for tracking the request.</param>
    /// <returns>A formatted error message.</returns>
    private static string FormatErrorMessageForResponse(
        Exception exception,
        string operationName,
        string correlationId)
    {
        string baseMessage = exception switch
        {
            HttpRequestException httpEx when httpEx.StatusCode.HasValue
                && (int)httpEx.StatusCode.Value >= 400
                && (int)httpEx.StatusCode.Value < 500 =>
                    $"Client error during {operationName}: {exception.Message}",

            HttpRequestException httpEx when httpEx.StatusCode.HasValue
                && (int)httpEx.StatusCode.Value >= 500 =>
                    $"Server error during {operationName}: {exception.Message}",

            OperationCanceledException or TaskCanceledException =>
                $"Request timeout during {operationName}",

            ArgumentException =>
                $"Invalid argument during {operationName}: {exception.Message}",

            _ => $"Unexpected error during {operationName}: {exception.Message}"
        };

        return $"{baseMessage} (Ref: {correlationId})";
    }

    /// <summary>
    /// Determines the appropriate HTTP status code for an exception.
    /// </summary>
    /// <param name="exception">The exception to analyze.</param>
    /// <returns>The appropriate HTTP status code for the exception.</returns>
    public static HttpStatusCode DetermineStatusCodeForException(Exception exception)
    {
        return exception switch
        {
            // If it's already an HttpRequestException with a status code, use that
            HttpRequestException httpEx when httpEx.StatusCode.HasValue => httpEx.StatusCode.Value,

            // Common exception mappings
            OperationCanceledException or TaskCanceledException => HttpStatusCode.RequestTimeout,
            ArgumentException or ArgumentNullException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            NotImplementedException => HttpStatusCode.NotImplemented,
            InvalidOperationException => HttpStatusCode.BadRequest,
            TimeoutException => HttpStatusCode.RequestTimeout,

            // Default fallback
            _ => HttpStatusCode.InternalServerError
        };
    }

    /// <summary>
    /// Generic error handler that can be used to wrap operations and consistently handle exceptions.
    /// </summary>
    /// <typeparam name="TResult">The type of the result from the operation.</typeparam>
    /// <param name="operation">The operation function to execute.</param>
    /// <param name="logger">The logger to use for logging errors.</param>
    /// <param name="operationName">A descriptive name for the operation being performed.</param>
    /// <param name="correlationId">A correlation ID for tracking the request.</param>
    /// <param name="contextData">Additional context data to include in logs.</param>
    /// <param name="fallbackResult">Optional fallback result to use if the operation fails.</param>
    /// <returns>The result of the operation or the fallback value if an exception occurs.</returns>
    public static async Task<TResult> ExecuteWithErrorHandlingAsync<TResult>(
        Func<Task<TResult>> operation,
        ILogger logger,
        string operationName,
        string correlationId,
        Dictionary<string, object>? contextData = null,
        TResult? fallbackResult = default)
    {
        try
        {
            return await operation().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Log the exception with rich context
            LogException(ex, logger, operationName, correlationId, contextData);

            // If a fallback result was provided, return it
            if (fallbackResult != null)
                return fallbackResult;

            // Otherwise, rethrow the enriched exception
            throw EnrichException(ex, $"Error in {operationName}", contextData);
        }
    }

    /// <summary>
    /// Logs an exception with consistent formatting and appropriate log level.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="logger">The logger to use.</param>
    /// <param name="operationName">The name of the operation that failed.</param>
    /// <param name="correlationId">The correlation ID for tracking.</param>
    /// <param name="contextData">Additional context data to include.</param>
    public static void LogException(
        Exception exception,
        ILogger logger,
        string operationName,
        string correlationId,
        Dictionary<string, object>? contextData = null)
    {
        // Determine the appropriate log level
        var logLevel = LoggingUtility.DetermineLogLevelForException(exception);

        // Create base context data
        var context = new Dictionary<string, object>
        {
            ["OperationName"] = operationName,
            ["CorrelationId"] = correlationId,
            ["ExceptionType"] = exception.GetType().Name,
            ["Timestamp"] = DateTime.UtcNow
        };

        // Add any additional context data
        if (contextData != null)
        {
            foreach (var kvp in contextData)
            {
                context[kvp.Key] = kvp.Value;
            }
        }

        // Log the exception with all context
        logger.Log(
            logLevel,
            exception,
            "Error during {OperationName}: {ErrorMessage} [CorrelationId: {CorrelationId}]",
            operationName,
            exception.Message,
            correlationId);
    }
}