using HttpClientUtility.Utilities.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HttpClientUtility.RequestResult;

/// <summary>
/// Implementation of IHttpRequestResultService that caches HTTP responses using IMemoryCache.
/// </summary>
/// <remarks>
/// This service implements standardized logging and error handling patterns
/// to ensure consistent behavior across all HTTP requests that use caching.
/// </remarks>
public sealed class HttpRequestResultServiceCache(
    IHttpRequestResultService service,
    ILogger<HttpRequestResultServiceCache> logger,
    IMemoryCache cache) : IHttpRequestResultService
{
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<HttpRequestResultServiceCache> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpRequestResultService _service = service ?? throw new ArgumentNullException(nameof(service));

    /// <summary>
    /// Sends an HTTP request asynchronously with caching support and returns the result.
    /// </summary>
    /// <typeparam name="T">The type of the expected response data</typeparam>
    /// <param name="statusCall">The HTTP request result object containing request details</param>
    /// <param name="memberName">Name of the calling member (automatically populated)</param>
    /// <param name="filePath">File path of the calling code (automatically populated)</param>
    /// <param name="lineNumber">Line number of the calling code (automatically populated)</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation with the HTTP request result</returns>
    /// <remarks>
    /// This method implements an error handling strategy for caching operations:
    /// 1. Cache read errors are logged but not propagated - the request continues as if not cached
    /// 2. Cache write errors are logged but not propagated - the result is still returned
    /// 3. The underlying HTTP request retains its error handling behavior from the wrapped service
    /// </remarks>
    public async Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> statusCall,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var cacheKey = statusCall.RequestPath;

        // Store the operation name for consistent logging
        string operationName = $"Cached HTTP {statusCall.RequestMethod.Method}";
        string correlationId = statusCall.CorrelationId;

        // Log the beginning of the request processing
        _logger.LogDebug(
            "Processing {Operation} request, checking cache for {CacheKey} [CorrelationId: {CorrelationId}]",
            operationName,
            LoggingUtility.SanitizeUrl(cacheKey),
            correlationId);

        // Check if we should try to use the cache
        if (statusCall.CacheDurationMinutes > 0)
        {
            try
            {
                if (_cache.TryGetValue(cacheKey, out HttpRequestResult<T>? cachedResult))
                {
                    if (cachedResult != null && cachedResult.ResponseResults != null)
                    {
                        stopwatch.Stop();

                        // Add context to the cached result
                        cachedResult.RequestContext["CacheHit"] = true;
                        cachedResult.RequestContext["CacheAge"] = cachedResult.ResultAge;

                        _logger.LogInformation(
                            "Cache hit for {Operation} request {CacheKey}, returning cached response from {CacheAge} [CorrelationId: {CorrelationId}]",
                            operationName,
                            LoggingUtility.SanitizeUrl(cacheKey),
                            cachedResult.ResultAge,
                            correlationId);

                        return cachedResult;
                    }
                }

                _logger.LogDebug(
                    "Cache miss for {Operation} request {CacheKey}, proceeding with actual request [CorrelationId: {CorrelationId}]",
                    operationName,
                    LoggingUtility.SanitizeUrl(cacheKey),
                    correlationId);
            }
            catch (Exception ex)
            {
                // Create context data for logging
                var contextData = new Dictionary<string, object>
                {
                    ["CacheKey"] = LoggingUtility.SanitizeUrl(cacheKey),
                    ["CacheDurationMinutes"] = statusCall.CacheDurationMinutes
                };

                // Log the error with rich context
                ErrorHandlingUtility.LogException(
                    ex,
                    _logger,
                    "Cache Read Operation",
                    correlationId,
                    contextData);

                _logger.LogWarning(
                    "Failed to read from cache, proceeding with actual request [CorrelationId: {CorrelationId}]",
                    correlationId);
            }
        }

        // If the result is not cached, make the actual HTTP request using the wrapped service
        statusCall = await _service.HttpSendRequestResultAsync(
            statusCall, memberName, filePath, lineNumber, ct).ConfigureAwait(false);

        statusCall.CompletionDate = DateTime.UtcNow;

        // If caching is enabled and the request was successful, cache the result
        if (statusCall.CacheDurationMinutes > 0 && statusCall.ResponseResults != null)
        {
            try
            {
                var cacheDuration = TimeSpan.FromMinutes(statusCall.CacheDurationMinutes);

                _cache.Set(cacheKey, statusCall, cacheDuration);

                _logger.LogDebug(
                    "Cached {Operation} response for {CacheKey} with duration {CacheDuration} minutes [CorrelationId: {CorrelationId}]",
                    operationName,
                    LoggingUtility.SanitizeUrl(cacheKey),
                    statusCall.CacheDurationMinutes,
                    correlationId);

                // Add caching context to the result
                statusCall.RequestContext["CacheStored"] = true;
                statusCall.RequestContext["CacheDuration"] = cacheDuration;
            }
            catch (Exception ex)
            {
                // Create context data for logging
                var contextData = new Dictionary<string, object>
                {
                    ["CacheKey"] = LoggingUtility.SanitizeUrl(cacheKey),
                    ["CacheDurationMinutes"] = statusCall.CacheDurationMinutes,
                    ["ResponseStatus"] = statusCall.StatusCode
                };

                // Log the error with rich context
                ErrorHandlingUtility.LogException(
                    ex,
                    _logger,
                    "Cache Write Operation",
                    correlationId,
                    contextData);

                _logger.LogWarning(
                    "Failed to store result in cache, but returning the successful response [CorrelationId: {CorrelationId}]",
                    correlationId);

                // Add caching context to the result
                statusCall.RequestContext["CacheStored"] = false;
                statusCall.RequestContext["CacheError"] = ex.Message;
            }
        }

        // Record the total time spent in this cache wrapper
        stopwatch.Stop();
        statusCall.RequestContext["CacheLayerDurationMs"] = stopwatch.ElapsedMilliseconds;

        return statusCall;
    }
}
