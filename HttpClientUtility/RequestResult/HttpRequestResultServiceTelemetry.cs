using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HttpClientUtility.RequestResult;


/// <summary>
/// Class HttpRequestResultServiceTelemetry adds telemetry to the IHttpRequestResultService implementation
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HttpRequestResultServiceTelemetry"/> class
/// </remarks>
/// <param name="logger">ILogger instance</param>
/// <param name="service">IHttpRequestResultService instance</param>
public class HttpRequestResultServiceTelemetry(ILogger<HttpRequestResultServiceTelemetry> logger,
    IHttpRequestResultService service) : IHttpRequestResultService
{
    /// <summary>
    /// Sends an HTTP request asynchronously while capturing telemetry data.
    /// </summary>
    /// <typeparam name="T">The type of the expected response data</typeparam>
    /// <param name="statusCall">The HTTP request result object containing request details</param>
    /// <param name="memberName">Name of the calling member (automatically populated)</param>
    /// <param name="filePath">File path of the calling code (automatically populated)</param>
    /// <param name="lineNumber">Line number of the calling code (automatically populated)</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation with the HTTP request result</returns>
    public async Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> statusCall,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        CancellationToken ct = default)
    {
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.HttpSendRequestResultAsync(statusCall,
                memberName, filePath, lineNumber, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            statusCall.ErrorList.Add($"Telemetry:GetAsync:Exception:{ex.Message}");
            logger.LogCritical("Telemetry:GetAsync:Exception:{Message}", ex.Message);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.UtcNow;
        return statusCall;
    }
}
