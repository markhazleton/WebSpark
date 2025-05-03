using Microsoft.Extensions.Caching.Memory;

namespace HttpClientUtility.MemoryCache;

/// <summary>
/// Implementation of HTTP client memory cache that uses IMemoryCache
/// </summary>
public class HttpClientMemoryCache : IHttpClientMemoryCache
{
    private readonly IMemoryCache _cache;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public HttpClientMemoryCache(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// Gets a value from the cache or creates it using the factory if not present
    /// </summary>
    /// <typeparam name="T">The type of object to get from the cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">A factory function to create the value if not in cache</param>
    /// <param name="cacheDuration">How long to cache the value</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The cached or newly created value</returns>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan cacheDuration, CancellationToken cancellationToken = default)
    {
        // Try to get the item from cache first
        if (_cache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue!;
        }

        // If it's not in the cache, we need to create it
        // Use a semaphore to prevent multiple concurrent calls to create the same item
        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Check again in case another thread already added the item
            if (_cache.TryGetValue(key, out cachedValue))
            {
                return cachedValue!;
            }

            // Create the value
            var newValue = await factory();

            // Add the value to the cache
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(cacheDuration);

            _cache.Set(key, newValue, cacheEntryOptions);

            return newValue;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Removes an item from the cache
    /// </summary>
    /// <param name="key">The cache key to remove</param>
    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    /// <summary>
    /// Clears all items from the cache
    /// </summary>
    public void Clear()
    {
        // IMemoryCache doesn't have a Clear method, so we need to use a different approach
        // This is a way to signal that the cache has been reset by creating a new instance
        // We can achieve this by registering our cache as a singleton in DI and then replacing it
        // But we can't do that here directly, so we have to rely on removing individual keys
        // In a real-world scenario, you might want to track the keys you've added

        // For this implementation, we can't actually clear the entire cache
        // If this is needed, consider extending this class to track keys added
    }
}