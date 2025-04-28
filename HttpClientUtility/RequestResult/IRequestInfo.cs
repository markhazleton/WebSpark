namespace HttpClientUtility.RequestResult;

/// <summary>
/// Interface representing information about an HTTP request.
/// Contains details about the request such as path, method, headers, and body.
/// </summary>
public interface IRequestInfo
{
    /// <summary>
    /// Gets or sets the iteration count for the request, used for retry tracking.
    /// </summary>
    int Iteration { get; set; }

    /// <summary>
    /// Gets or sets the request path or URL.
    /// </summary>
    string RequestPath { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method used for the request (GET, POST, etc.).
    /// </summary>
    HttpMethod RequestMethod { get; set; }

    /// <summary>
    /// Gets or sets the request body content.
    /// </summary>
    StringContent? RequestBody { get; set; }

    /// <summary>
    /// Gets or sets the request headers as a dictionary of key-value pairs.
    /// </summary>
    Dictionary<string, string>? RequestHeaders { get; set; }
}
