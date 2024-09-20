namespace HttpClientUtility.SendService;

/// <summary>
/// Concrete class for HTTP requests with specific response type.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
public class HttpClientSendRequest<T> : HttpClientSendRequestBase
{
    /// <summary>
    /// Gets or sets the response results.
    /// </summary>
    public T? ResponseResults { get; set; }
    /// <summary>
    /// Initializes a new instance of the HttpClientSendRequest class.
    /// </summary>
    public HttpClientSendRequest() : base() { }
    /// <summary>
    ///  Initializes a new instance of the HttpClientSendRequest class.
    /// </summary>
    /// <param name="it"></param>
    /// <param name="path"></param>
    public HttpClientSendRequest(int it, string path) : base()
    {
        Iteration = it;
        RequestPath = path;
    }
    /// <summary>
    /// Initializes a new instance of the HttpClientSendRequest class.
    /// </summary>
    /// <param name="request"></param>
    public HttpClientSendRequest(HttpClientSendRequestBase request) : base()
    {
        Iteration = request.Iteration;
        RequestPath = request.RequestPath;
        RequestMethod = request.RequestMethod;
        RequestBody = request.RequestBody;
        RequestHeaders = request.RequestHeaders;
        CacheDurationMinutes = request.CacheDurationMinutes;
        Retries = request.Retries;
    }
}
