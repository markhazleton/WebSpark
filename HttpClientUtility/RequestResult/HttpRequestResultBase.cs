using HttpClientUtility.Utilities.Logging;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HttpClientUtility.RequestResult;


/// <summary>
/// Abstract base class implementing template method pattern for HTTP requests.
/// </summary>
public abstract class HttpRequestResultBase : IRequestInfo, IResultInfo, IErrorInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequestResultBase"/> class.
    /// </summary>
    protected HttpRequestResultBase()
    {
        Id = 1;
        RequestPath = string.Empty; // Initialize to avoid nullability warning
        CorrelationId = Guid.NewGuid().ToString(); // Generate a unique correlation ID for tracking
        RequestStartTimestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets or sets the cache duration in minutes.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 1;

    /// <summary>
    /// Gets or sets the completion date of the request.
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Gets or sets the elapsed time in milliseconds for the request.
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the list of errors occurred during the request.
    /// </summary>
    public List<string> ErrorList { get; set; } = [];

    /// <summary>
    /// Gets or sets the ID of the request.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the iteration of the request.
    /// </summary>
    public int Iteration { get; set; }

    /// <summary>
    /// Gets or sets the request body.
    /// </summary>
    public StringContent? RequestBody { get; set; }

    /// <summary>
    /// Gets or sets the request headers.
    /// </summary>
    public Dictionary<string, string>? RequestHeaders { get; set; } = [];

    /// <summary>
    /// Gets or sets the request method.
    /// </summary>
    public HttpMethod RequestMethod { get; set; } = HttpMethod.Get;

    /// <summary>
    /// Gets or sets the request path.
    /// </summary>
    public string RequestPath { get; set; }

    /// <summary>
    /// Gets the age of the result in hours, minutes, and seconds.
    /// </summary>
    public string ResultAge
    {
        get
        {
            if (!CompletionDate.HasValue)
            {
                return "Result Cache date is null.";
            }
            TimeSpan timeDifference = DateTime.UtcNow - CompletionDate.Value;
            return $"Age: {timeDifference.Hours} hours, {timeDifference.Minutes} minutes, {timeDifference.Seconds} seconds.";
        }
    }

    /// <summary>
    /// Gets or sets the number of retries for the request.
    /// </summary>
    public int Retries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID for tracking this request through the system.
    /// </summary>
    /// <remarks>
    /// The correlation ID is a unique identifier that follows the request as it flows through different
    /// components of the system, making it easier to trace the entire request lifecycle in logs.
    /// </remarks>
    public string CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the request started.
    /// </summary>
    public DateTime RequestStartTimestamp { get; set; }

    /// <summary>
    /// Gets or sets additional context information for this request.
    /// </summary>
    /// <remarks>
    /// This can be used to store any additional metadata or context about the request
    /// that might be useful for debugging or logging purposes.
    /// </remarks>
    public Dictionary<string, object> RequestContext { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether debugging is enabled for this request.
    /// </summary>
    /// <remarks>
    /// When debugging is enabled, more detailed information may be captured and logged.
    /// </remarks>
    public bool IsDebugEnabled { get; set; } = false;

    /// <summary>
    /// Gets a sanitized version of the request path for logging purposes.
    /// </summary>
    public string SafeRequestPath => LoggingUtility.SanitizeUrl(RequestPath);

    /// <summary>
    /// Logs detailed information about this request.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    public void LogRequestDetails(ILogger logger)
    {
        // Only log detailed request information at Debug level
        if (!logger.IsEnabled(LogLevel.Debug))
            return;

        var methodName = RequestMethod?.Method ?? "Unknown";

        logger.LogDebug(
            "HTTP Request Details [CorrelationId: {CorrelationId}]:{NewLine}" +
            "  Method: {Method}{NewLine}" +
            "  Path: {Path}{NewLine}" +
            "  Started: {Started}{NewLine}" +
            "  Headers: {HeaderCount}{NewLine}" +
            "  Status: {StatusCode}",
            CorrelationId,
            Environment.NewLine,
            methodName,
            SafeRequestPath,
            RequestStartTimestamp,
            RequestHeaders?.Count ?? 0,
            StatusCode);
    }

    /// <summary>
    /// Adds an error message to the error list with the correlation ID for reference.
    /// </summary>
    /// <param name="errorMessage">The error message to add.</param>
    public void AddError(string errorMessage)
    {
        ErrorList.Add($"{errorMessage} [CorrelationId: {CorrelationId}]");
    }

    /// <summary>
    /// Adds exception details to the error list and returns the exception enriched with context.
    /// </summary>
    /// <param name="exception">The exception to process.</param>
    /// <param name="contextMessage">Additional context message to include.</param>
    /// <returns>An enriched exception with request context.</returns>
    public Exception ProcessException(Exception exception, string contextMessage)
    {
        // Add to the error list
        AddError($"{contextMessage}: {exception.Message}");

        // Create context data for the exception
        var contextData = new Dictionary<string, object>
        {
            ["RequestPath"] = SafeRequestPath,
            ["RequestMethod"] = RequestMethod?.Method ?? "Unknown",
            ["CorrelationId"] = CorrelationId,
            ["StatusCode"] = StatusCode
        };

        // Add any custom context data from the request
        foreach (var item in RequestContext)
        {
            contextData[item.Key] = item.Value;
        }

        // Return the enriched exception
        return ErrorHandlingUtility.EnrichException(exception, contextMessage, contextData);
    }
}
