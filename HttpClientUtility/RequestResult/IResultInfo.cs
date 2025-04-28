using System.Net;

namespace HttpClientUtility.RequestResult;

/// <summary>
/// Interface representing information about an HTTP response result.
/// Contains details about the response such as status code, timing, and completion information.
/// </summary>
public interface IResultInfo
{
    /// <summary>
    /// Gets or sets the date and time when the request was completed.
    /// </summary>
    DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Gets or sets the total time in milliseconds that the request took to complete.
    /// </summary>
    long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets a string representation of how old the result is.
    /// </summary>
    string ResultAge { get; }
}
