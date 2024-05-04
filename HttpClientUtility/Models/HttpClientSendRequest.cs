using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace HttpClientUtility.Models;

/// <summary>
/// Class to store the results of an HTTP GET call.
/// </summary>
public class HttpClientSendRequest<T>
{
    /// <summary>
    /// Default constructor to initialize the iteration and status path.
    /// </summary>
    public HttpClientSendRequest()
    {
        Iteration = 0;
        RequestPath = string.Empty;
    }

    /// <summary>
    /// Constructor to initialize the iteration and status path from another instance of HttpClientSendRequest.
    /// </summary>
    /// <param name="statusCall">An instance of HttpClientSendRequest.</param>
    public HttpClientSendRequest(HttpClientSendRequest<T> statusCall)
    {
        Iteration = statusCall.Iteration;
        RequestPath = statusCall.RequestPath;
        ResponseResults = statusCall.ResponseResults;
        ErrorList = statusCall.ErrorList;
        StatusCode = statusCall.StatusCode;
        CompletionDate = statusCall.CompletionDate;
        ElapsedMilliseconds = statusCall.ElapsedMilliseconds;
        Id = statusCall.Id;
        RequestBody = statusCall.RequestBody;
        CacheDurationMinutes = statusCall.CacheDurationMinutes;
        Retries = statusCall.Retries;
    }

    /// <summary>
    /// Constructor to initialize the iteration and status path from given values.
    /// </summary>
    /// <param name="it">Iteration number of the HTTP GET call.</param>
    /// <param name="path">Status path of the HTTP GET call.</param>
    public HttpClientSendRequest(int it, string path)
    {
        Iteration = it;
        RequestPath = path;
    }

    public int CacheDurationMinutes { get; set; } = 1;

    /// <summary>
    /// Property to store the completion date and time of the HTTP GET call.
    /// </summary>
    [DisplayFormat(DataFormatString = "{0:yyyy.MM.dd hh:mm:ss.ffff}")]
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Property to store the elapsed time in milliseconds of the HTTP GET call.
    /// </summary>
    public long ElapsedMilliseconds { get; set; }
    /// <summary>
    /// Error Message if something goes wrong, usually null
    /// </summary>
    public List<string> ErrorList { get; set; } = [];

    /// <summary>
    /// Id for this record
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key, Column(Order = 0)]
    public int Id { get; set; }

    /// <summary>
    /// Property to store the iteration number of the HTTP GET call.
    /// </summary>
    public int Iteration { get; set; }

    /// <summary>
    /// The Body of the Http Client Request
    /// </summary>
    [NotMapped]
    public StringContent? RequestBody { get; set; }

    [NotMapped]
    public HttpMethod RequestMethod { get; set; } = HttpMethod.Get;

    /// <summary>
    /// Property to store the status path of the HTTP GET call.
    /// </summary>
    public string RequestPath { get; set; }

    /// <summary>
    /// Property to store the results of the HTTP Client Request.
    /// </summary>
    [NotMapped]
    public T? ResponseResults { get; set; }

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

    public HttpStatusCode StatusCode { get; set; }
}

