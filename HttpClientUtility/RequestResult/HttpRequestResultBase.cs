using System.Net;

namespace HttpClientUtility.RequestResult;


/// <summary>
/// Abstract base class implementing template method pattern for HTTP requests.
/// </summary>
public abstract class HttpRequestResultBase : IRequestInfo, IResultInfo, IErrorInfo
{
    protected HttpRequestResultBase()
    {
        Id = 1;
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
}
