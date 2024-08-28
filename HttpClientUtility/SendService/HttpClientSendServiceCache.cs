using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HttpClientUtility.SendService;

/// <summary>
/// Implementation of IHttpClientSendService that caches HTTP responses using IMemoryCache.
/// </summary>
public sealed class HttpClientSendServiceCache(
    IHttpClientSendService service,
    ILogger<HttpClientSendServiceCache> logger,
    IMemoryCache cache) : IHttpClientSendService
{
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<HttpClientSendServiceCache> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpClientSendService _service = service ?? throw new ArgumentNullException(nameof(service));
    /// <summary>
    /// HttpClientSendServiceCache Constructor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="statusCall"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<HttpClientSendRequest<T>> HttpClientSendAsync<T>(HttpClientSendRequest<T> statusCall, CancellationToken ct)
    {
        var cacheKey = statusCall.RequestPath;
        if (statusCall.CacheDurationMinutes > 0)
        {
            try
            {
                if (_cache.TryGetValue(cacheKey, out HttpClientSendRequest<T>? cachedResult))
                {
                    if (cachedResult != null)
                    {
                        return cachedResult;
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
        statusCall = await _service.HttpClientSendAsync(statusCall, ct);
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
