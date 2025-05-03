using System.Runtime.Serialization;

namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Exception thrown when errors occur during GitHub API operations
/// </summary>
[Serializable]
public class GitHubApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the GitHubApiException class
    /// </summary>
    public GitHubApiException() { }

    /// <summary>
    /// Initializes a new instance of the GitHubApiException class with a specified error message
    /// </summary>
    /// <param name="message">Error message that explains the reason for the exception</param>
    public GitHubApiException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the GitHubApiException class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">Error message that explains the reason for the exception</param>
    /// <param name="innerException">Exception that is the cause of the current exception</param>
    public GitHubApiException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the GitHubApiException class with a specified error message and status code
    /// </summary>
    /// <param name="message">Error message that explains the reason for the exception</param>
    /// <param name="statusCode">HTTP status code returned by the GitHub API</param>
    public GitHubApiException(string message, System.Net.HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = (int)statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the GitHubApiException class with serialized data
    /// </summary>
    /// <param name="info">SerializationInfo that holds the serialized object data</param>
    /// <param name="context">StreamingContext that contains contextual information</param>
    protected GitHubApiException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    /// <summary>
    /// HTTP status code returned by the GitHub API (if available)
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// GitHub API rate limit information (if available)
    /// </summary>
    public RateLimitInfo? RateLimitInfo { get; set; }
}