using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace HttpClientUtility.RequestResult;

/// <summary>
/// Implementation of IHttpRequestResultService that caches HTTP responses using IMemoryCache.
/// </summary>
public sealed class HttpRequestResultServiceCache(
    IHttpRequestResultService service,
    ILogger<HttpRequestResultServiceCache> logger,
    IMemoryCache cache) : IHttpRequestResultService
{
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<HttpRequestResultServiceCache> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpRequestResultService _service = service ?? throw new ArgumentNullException(nameof(service));
    /// <summary>
    /// HttpRequestResultServiceCache Constructor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="statusCall"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> statusCall,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        CancellationToken ct = default)
    {
        var cacheKey = statusCall.RequestPath;
        if (statusCall.CacheDurationMinutes > 0)
        {
            try
            {
                if (_cache.TryGetValue(cacheKey, out HttpRequestResult<T>? cachedResult))
                {
                    if (cachedResult != null)
                    {
                        if(cachedResult.ResponseResults != null)
                        {
                            return cachedResult;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log, report, or take appropriate action)
                _logger.LogError(ex, "Error while attempting to get cache item with key: {cacheKey}", cacheKey);
            }
        }
        // If the result is not cached, make the actual HTTP request using the wrapped service
        // and store the result in the cache before returning it
        statusCall = await _service.HttpSendRequestResultAsync(statusCall, memberName, filePath, lineNumber, ct);
        statusCall.CompletionDate = DateTime.UtcNow;
        if (statusCall.CacheDurationMinutes > 0)
        {
            try
            {
                _cache.Set(cacheKey, statusCall, TimeSpan.FromMinutes(statusCall.CacheDurationMinutes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while attempting to set cache item with key: {cacheKey}", cacheKey);
            }
        }
        return statusCall;
    }
}
