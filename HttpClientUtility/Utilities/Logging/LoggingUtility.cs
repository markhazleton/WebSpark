using Microsoft.Extensions.Logging;
using System.Text;

namespace HttpClientUtility.Utilities.Logging;

/// <summary>
/// Provides standardized logging methods and utilities for consistent logging patterns across the library.
/// </summary>
public static class LoggingUtility
{
    /// <summary>
    /// Sanitizes a URL to prevent sensitive information from being logged.
    /// </summary>
    /// <param name="url">The URL to sanitize.</param>
    /// <returns>A sanitized version of the URL with sensitive information redacted.</returns>
    public static string SanitizeUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        try
        {
            var uri = new Uri(url);

            // If no query parameters, return as is
            if (string.IsNullOrEmpty(uri.Query))
                return url;

            // Parse query string
            var query = uri.Query.TrimStart('?').Split('&');
            var sanitizedQuery = new StringBuilder();
            bool isFirst = true;

            foreach (var param in query)
            {
                if (string.IsNullOrEmpty(param))
                    continue;

                var keyValue = param.Split('=');
                if (keyValue.Length < 2)
                    continue;

                var key = keyValue[0];

                // Check if this is a sensitive parameter that should be redacted
                if (IsSensitiveParameter(key))
                {
                    if (!isFirst)
                        sanitizedQuery.Append('&');

                    sanitizedQuery.Append(key).Append("=***REDACTED***");
                    isFirst = false;
                }
                else
                {
                    if (!isFirst)
                        sanitizedQuery.Append('&');

                    sanitizedQuery.Append(param);
                    isFirst = false;
                }
            }

            // Rebuild the URL with the sanitized query
            var sanitizedUrl = $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";
            if (sanitizedQuery.Length > 0)
                sanitizedUrl += $"?{sanitizedQuery}";

            return sanitizedUrl;
        }
        catch (Exception)
        {
            // If URL cannot be parsed, return a partially redacted version
            return url.Contains("?")
                ? url.Substring(0, url.IndexOf('?') + 1) + "***REDACTED***"
                : url;
        }
    }

    /// <summary>
    /// Determines if a parameter name might contain sensitive information.
    /// </summary>
    /// <param name="paramName">The parameter name to check.</param>
    /// <returns>True if the parameter might contain sensitive information; otherwise, false.</returns>
    private static bool IsSensitiveParameter(string paramName)
    {
        var sensitiveKeywords = new[]
        {
            "key", "token", "secret", "password", "pwd", "credential", "auth",
            "apikey", "api_key", "access", "private", "confidential", "sensitive"
        };

        return sensitiveKeywords.Any(keyword =>
            paramName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>
    /// Creates a dictionary of additional properties that should be included in structured logs.
    /// </summary>
    /// <param name="correlationId">The correlation ID for request tracing.</param>
    /// <param name="operationName">The name of the operation being performed.</param>
    /// <param name="additionalProperties">Any additional properties to include.</param>
    /// <returns>A dictionary containing the properties for structured logging.</returns>
    public static Dictionary<string, object> CreateLoggingContext(
        string correlationId,
        string operationName,
        Dictionary<string, object>? additionalProperties = null)
    {
        var context = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Operation"] = operationName,
            ["Timestamp"] = DateTime.UtcNow
        };

        if (additionalProperties != null)
        {
            foreach (var property in additionalProperties)
            {
                context[property.Key] = property.Value;
            }
        }

        return context;
    }

    /// <summary>
    /// Logs the beginning of an HTTP request with appropriate information.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="method">The HTTP method being used.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="correlationId">The correlation ID for tracing.</param>
    public static void LogRequestStart(ILogger logger, string method, string requestUri, string correlationId)
    {
        logger.LogInformation(
            "Starting HTTP {Method} request to {RequestUri} [CorrelationId: {CorrelationId}]",
            method,
            SanitizeUrl(requestUri),
            correlationId);
    }

    /// <summary>
    /// Logs the completion of an HTTP request with appropriate information.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="method">The HTTP method that was used.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds.</param>
    /// <param name="correlationId">The correlation ID for tracing.</param>
    public static void LogRequestCompletion(
        ILogger logger,
        string method,
        string requestUri,
        int statusCode,
        long elapsedMilliseconds,
        string correlationId)
    {
        var level = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        logger.Log(
            level,
            "Completed HTTP {Method} request to {RequestUri} with status code {StatusCode} in {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
            method,
            SanitizeUrl(requestUri),
            statusCode,
            elapsedMilliseconds,
            correlationId);
    }

    /// <summary>
    /// Logs an exception that occurred during an HTTP request with rich context.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="method">The HTTP method that was used.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="correlationId">The correlation ID for tracing.</param>
    /// <param name="additionalContext">Any additional context to include.</param>
    public static void LogRequestException(
        ILogger logger,
        Exception exception,
        string method,
        string requestUri,
        string correlationId,
        Dictionary<string, object>? additionalContext = null)
    {
        var context = CreateLoggingContext(correlationId, $"{method} {SanitizeUrl(requestUri)}", additionalContext);

        // Determine appropriate log level based on exception type
        var level = DetermineLogLevelForException(exception);

        // Create a structured log with rich context
        logger.Log(
            level,
            exception,
            "Error during HTTP {Method} request to {RequestUri}: {ErrorMessage} [CorrelationId: {CorrelationId}]",
            method,
            SanitizeUrl(requestUri),
            exception.Message,
            correlationId);
    }

    /// <summary>
    /// Determines the appropriate log level for different types of exceptions.
    /// </summary>
    /// <param name="exception">The exception to categorize.</param>
    /// <returns>The appropriate LogLevel for the exception.</returns>
    public static LogLevel DetermineLogLevelForException(Exception exception)
    {
        return exception switch
        {
            // Timeout and cancellation are expected in some scenarios
            OperationCanceledException or TaskCanceledException => LogLevel.Warning,

            // HTTP exceptions with 4xx status are client errors (warnings)
            HttpRequestException httpEx when httpEx.StatusCode.HasValue && (int)httpEx.StatusCode.Value >= 400 && (int)httpEx.StatusCode.Value < 500
                => LogLevel.Warning,

            // HTTP exceptions with 5xx status are server errors (errors)
            HttpRequestException => LogLevel.Error,

            // ArgumentExceptions are programming errors
            ArgumentException or ArgumentNullException => LogLevel.Error,

            // Authorization failures
            UnauthorizedAccessException => LogLevel.Warning,

            // Critical failures
            OutOfMemoryException or StackOverflowException => LogLevel.Critical,

            // Default to Error for unknown exceptions
            _ => LogLevel.Error
        };
    }
}