
namespace WebSpark.Core.Infrastructure.Middleware;

/// <summary>
/// 
/// </summary>
public class StatusCodeException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="statusCode"></param>
    public StatusCodeException(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// 
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }
}


