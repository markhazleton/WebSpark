namespace HttpClientUtility.MemoryCache;

/// <summary>
/// Interface for memory cache management operations.
/// Provides methods for storing, retrieving, and managing cached items.
/// </summary>
public interface IMemoryCacheManager
{
    /// <summary>
    /// Clears all items from the cache.
    /// </summary>
    void Clear();

    /// <summary>
    /// Disposes the cache manager instance and releases resources.
    /// </summary>
    void Dispose();

    /// <summary>
    /// Gets an item from the cache. If the item doesn't exist, it will be acquired using the provided function and added to the cache.
    /// </summary>
    /// <typeparam name="T">Type of the item to retrieve</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="acquire">Function to acquire the item if it's not in the cache</param>
    /// <param name="cacheTime">Cache time in minutes (null for default)</param>
    /// <returns>The cached or newly acquired item</returns>
    T Get<T>(string key, Func<T> acquire, int? cacheTime = null);

    /// <summary>
    /// Gets all cache keys.
    /// </summary>
    /// <returns>List of all cache keys</returns>
    IList<string> GetKeys();

    /// <summary>
    /// Checks if an item exists in the cache.
    /// </summary>
    /// <param name="key">The cache key to check</param>
    /// <returns>True if the item exists, false otherwise</returns>
    bool IsSet(string key);

    /// <summary>
    /// Performs an action with a lock on the specified key.
    /// </summary>
    /// <param name="key">The key to lock on</param>
    /// <param name="expirationTime">The timespan after which the lock should expire</param>
    /// <param name="action">The action to perform while holding the lock</param>
    /// <returns>True if the action was performed successfully, false otherwise</returns>
    bool PerformActionWithLock(string key, TimeSpan expirationTime, Action action);

    /// <summary>
    /// Removes an item from the cache.
    /// </summary>
    /// <param name="key">The cache key to remove</param>
    void Remove(string key);

    /// <summary>
    /// Sets an item in the cache.
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="data">The data to cache</param>
    /// <param name="cacheTimeMinutes">Cache time in minutes</param>
    void Set(string key, object data, int cacheTimeMinutes);
}
