using System.ComponentModel.DataAnnotations;
using System.Net;

namespace HttpClientUtility;

/// <summary>
/// Represents the response content of an HTTP request.
/// </summary>
/// <typeparam name="T">The type of the content.</typeparam>
public class HttpResponseContent<T>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseContent{T}"/> class.
    /// </summary>
    /// <param name="content">The content of the HTTP response.</param>
    /// <param name="errorMessage">The error message of the HTTP response.</param>
    /// <param name="statusCode">The status code of the HTTP response.</param>
    /// <param name="isSuccess">A value indicating whether the HTTP response is successful.</param>
    private HttpResponseContent(T? content, string? errorMessage, HttpStatusCode statusCode, bool isSuccess)
    {
        StatusCode = statusCode;
        IsSuccess = isSuccess;
        if (isSuccess)
            Content = content;

        ErrorMessage = !isSuccess ? errorMessage : null;
    }

    /// <summary>
    /// Creates a failure instance of the <see cref="HttpResponseContent{T}"/> class.
    /// </summary>
    /// <param name="errorMessage">The error message of the HTTP response.</param>
    /// <param name="statusCode">The status code of the HTTP response.</param>
    /// <returns>A failure instance of the <see cref="HttpResponseContent{T}"/> class.</returns>
    public static HttpResponseContent<T> Failure(string errorMessage, HttpStatusCode statusCode) =>
        new(default, errorMessage, statusCode, false);

    /// <summary>
    /// Creates a success instance of the <see cref="HttpResponseContent{T}"/> class.
    /// </summary>
    /// <param name="content">The content of the HTTP response.</param>
    /// <param name="statusCode">The status code of the HTTP response.</param>
    /// <returns>A success instance of the <see cref="HttpResponseContent{T}"/> class.</returns>
    public static HttpResponseContent<T> Success(T content, HttpStatusCode statusCode) =>
        new(content, null, statusCode, true);

    /// <summary>
    /// Duration in minutes to cache the response.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 1;

    /// <summary>
    /// Property to store the completion date and time of the HTTP GET call.
    /// </summary>
    [DisplayFormat(DataFormatString = "{0:yyyy.MM.dd hh:mm:ss.ffff}")]
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Gets the content of the HTTP response.
    /// </summary>
    public T? Content { get; }

    /// <summary>
    /// Property to store the elapsed time in milliseconds of the HTTP GET call.
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// Gets the error message of the HTTP response, if any.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets a value indicating whether the HTTP response is successful.
    /// </summary>
    public bool IsSuccess { get; }
    public string ResultAge
    {
        get
        {
            if (!CompletionDate.HasValue)
            {
                return "Result Cache date is null.";
            }

            DateTime currentDate = DateTime.Now;
            DateTime inputDate = CompletionDate.Value;

            TimeSpan timeDifference = currentDate - inputDate;

            int days = timeDifference.Days;
            int hours = timeDifference.Hours;
            int minutes = timeDifference.Minutes;
            int seconds = timeDifference.Seconds;
            int milliseconds = timeDifference.Milliseconds;

            // Round up the milliseconds
            if (milliseconds >= 1)
            {
                seconds++;
            }

            // Perform carry over if necessary
            if (seconds >= 60)
            {
                minutes += seconds / 60;
                seconds %= 60;
            }

            if (minutes >= 60)
            {
                hours += minutes / 60;
                minutes %= 60;
            }

            if (hours >= 24)
            {
                days += hours / 24;
                hours %= 24;
            }

            return $"Result Cache Age: {days} days, {hours} hours, {minutes} minutes, {seconds} seconds.";
        }
    }

    /// <summary>
    /// Number of retires to get a successful HTTP Client Request.
    /// </summary>
    public int Retries { get; set; }
    /// <summary>
    /// Gets the status code of the HTTP response.
    /// </summary>
    public HttpStatusCode StatusCode { get; }
}
