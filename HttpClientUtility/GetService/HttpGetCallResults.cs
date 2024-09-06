using System.ComponentModel.DataAnnotations;
namespace HttpClientUtility.GetService;

/// <summary>
/// Represents the results of an HTTP GET call.
/// </summary>
public class HttpGetCallResults
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpGetCallResults"/> class.
    /// </summary>
    public HttpGetCallResults()
    {
        Iteration = 0;
        StatusPath = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpGetCallResults"/> class
    /// by copying the values from another <see cref="HttpGetCallResults"/> instance.
    /// </summary>
    /// <param name="statusCall">The <see cref="HttpGetCallResults"/> instance to copy from.</param>
    public HttpGetCallResults(HttpGetCallResults statusCall)
    {
        Iteration = statusCall.Iteration;
        StatusPath = statusCall.StatusPath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpGetCallResults"/> class
    /// with the specified iteration and status path.
    /// </summary>
    /// <param name="it">The iteration value.</param>
    /// <param name="path">The status path.</param>
    public HttpGetCallResults(int it, string path)
    {
        Iteration = it;
        StatusPath = path;
    }

    /// <summary>
    /// Gets or sets the completion date of the HTTP GET call.
    /// </summary>
    [DisplayFormat(DataFormatString = "{0:yyyy.MM.dd hh:mm:ss.ffff}")]
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Gets or sets the elapsed time in milliseconds for the HTTP GET call.
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the iteration number of the HTTP GET call.
    /// </summary>
    public int Iteration { get; set; }

    /// <summary>
    /// Gets or sets the status path of the HTTP GET call.
    /// </summary>
    public string StatusPath { get; set; }

    /// <summary>
    /// Gets or sets the dynamic status results of the HTTP GET call.
    /// </summary>
    public dynamic? StatusResults { get; set; }
}
