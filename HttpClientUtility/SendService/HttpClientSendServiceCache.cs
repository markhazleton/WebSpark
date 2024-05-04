using HttpClientUtility.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HttpClientUtility.SendService;

/// <summary>
/// Implementation of IHttpClientService that caches HTTP responses using IMemoryCache.
/// </summary>
public sealed class HttpClientSendServiceCache : Interfaces.IHttpClientService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<HttpClientSendServiceCache> _logger;
    private readonly Interfaces.IHttpClientService _service;

    public HttpClientSendServiceCache(Interfaces.IHttpClientService service, ILogger<HttpClientSendServiceCache> logger, IMemoryCache cache)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

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
        statusCall.CompletionDate = DateTime.Now;
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
