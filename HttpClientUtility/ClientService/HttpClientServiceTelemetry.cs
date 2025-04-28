using System.Diagnostics;
using System.Net;

namespace HttpClientUtility.ClientService;

/// <summary>
/// Represents a telemetry wrapper for an HttpClient service.
/// </summary>
public class HttpClientServiceTelemetry(IHttpClientService service) : IHttpClientService
{
    private readonly IHttpClientService service = service;

    /// <inheritdoc/>
    public HttpClient CreateConfiguredClient()
    {
        return service.CreateConfiguredClient();
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<TResult>> DeleteAsync<TResult>(Uri requestUri, CancellationToken cancellationToken = default)
    {
        HttpResponseContent<TResult> statusCall;
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.DeleteAsync<TResult>(requestUri, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            statusCall = HttpResponseContent<TResult>.Failure($"HTTP Request Exception: {ex.Message}", HttpStatusCode.ServiceUnavailable);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.UtcNow;
        return statusCall;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<T>> GetAsync<T>(Uri requestUri, CancellationToken cancellationToken)
    {
        HttpResponseContent<T> statusCall;
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.GetAsync<T>(requestUri, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            statusCall = HttpResponseContent<T>.Failure($"HTTP Request Exception: {ex.Message}", HttpStatusCode.ServiceUnavailable);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.UtcNow;
        return statusCall;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default)
    {
        HttpResponseContent<TResult> statusCall;
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.PostAsync<T, TResult>(requestUri, payload, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            statusCall = HttpResponseContent<TResult>.Failure($"HTTP Request Exception: {ex.Message}", HttpStatusCode.ServiceUnavailable);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.UtcNow;
        return statusCall;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        HttpResponseContent<TResult> statusCall;
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.PostAsync<T, TResult>(requestUri, payload, headers, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            statusCall = HttpResponseContent<TResult>.Failure($"HTTP Request Exception: {ex.Message}", HttpStatusCode.ServiceUnavailable);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.UtcNow;
        return statusCall;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<TResult>> PutAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default)
    {
        HttpResponseContent<TResult> statusCall;
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.PutAsync<T, TResult>(requestUri, payload, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            statusCall = HttpResponseContent<TResult>.Failure($"HTTP Request Exception: {ex.Message}", HttpStatusCode.ServiceUnavailable);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.UtcNow;
        return statusCall;
    }
}
