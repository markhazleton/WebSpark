namespace HttpClientUtility.RequestResult;

/// <summary>
/// Concrete class for HTTP requests with specific response type.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
public class HttpRequestResult<T> : HttpRequestResultBase
{
    /// <summary>
    /// Gets or sets the response results.
    /// </summary>
    public T? ResponseResults { get; set; }
    /// <summary>
    /// Initializes a new instance of the HttpRequestResult class.
    /// </summary>
    public HttpRequestResult() : base() { }
    /// <summary>
    ///  Initializes a new instance of the HttpRequestResult class.
    /// </summary>
    /// <param name="it"></param>
    /// <param name="path"></param>
    public HttpRequestResult(int it, string path) : base()
    {
        Iteration = it;
        RequestPath = path;
    }
    /// <summary>
    /// Initializes a new instance of the HttpRequestResult class.
    /// </summary>
    /// <param name="request"></param>
    public HttpRequestResult(HttpRequestResultBase request) : base()
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
