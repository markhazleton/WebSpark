using System.Net;

namespace HttpClientCrawler.Crawler;

/// <summary>
/// Represents errors that occur during web crawling operations.
/// </summary>
/// <remarks>
/// This exception provides detailed context about crawling errors, including 
/// HTTP status codes and the URL that caused the error.
/// </remarks>
public class CrawlException : Exception
{
    /// <summary>
    /// Gets the HTTP status code associated with this exception, if applicable.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the URL that was being crawled when the error occurred.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Gets the crawl depth at which the error occurred.
    /// </summary>
    public int Depth { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CrawlException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="url">The URL that was being crawled when the error occurred.</param>
    /// <param name="statusCode">The HTTP status code associated with this exception, if applicable.</param>
    /// <param name="depth">The crawl depth at which the error occurred.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CrawlException(string message, string url, HttpStatusCode statusCode, int depth = 0, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Url = url;
        Depth = depth;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CrawlException"/> class with a general error.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="url">The URL that was being crawled when the error occurred.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CrawlException(string message, string url, Exception innerException)
        : this(message, url, HttpStatusCode.InternalServerError, 0, innerException)
    {
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string containing the exception details including URL and status code.</returns>
    public override string ToString()
    {
        return $"{base.ToString()}\nURL: {Url}\nStatus Code: {StatusCode}\nDepth: {Depth}";
    }
}