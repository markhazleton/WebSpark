namespace HttpClientUtility.MemoryCache;

/// <summary>
/// Interface for an HTTP client memory cache
/// </summary>
public interface IHttpClientMemoryCache
{
    /// <summary>
    /// Gets a value from the cache or creates it using the factory if not present
    /// </summary>
    /// <typeparam name="T">The type of object to get from the cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">A factory function to create the value if not in cache</param>
    /// <param name="cacheDuration">How long to cache the value</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The cached or newly created value</returns>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan cacheDuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an item from the cache
    /// </summary>
    /// <param name="key">The cache key to remove</param>
    void Remove(string key);

    /// <summary>
    /// Clears all items from the cache
    /// </summary>
    void Clear();
}