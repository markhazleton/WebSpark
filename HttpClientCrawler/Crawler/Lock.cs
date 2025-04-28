using System.Collections.Concurrent;

namespace HttpClientCrawler.Crawler;

/// <summary>
/// Provides utilities for managing locks in multi-threaded crawler operations.
/// </summary>
/// <remarks>
/// This class helps prevent multiple crawl operations from simultaneously accessing 
/// the same URL, which can improve performance and reduce duplicate processing.
/// </remarks>
public class Lock
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    /// <summary>
    /// Gets or creates a lock for the specified key.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <returns>A semaphore that can be used to synchronize access to a resource.</returns>
    public static SemaphoreSlim GetLock(string key)
    {
        return _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }

    /// <summary>
    /// Acquires a lock asynchronously.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="ct">Cancellation token to stop the wait operation.</param>
    /// <returns>A task representing the asynchronous operation with a disposable lock.</returns>
    public static async Task<IDisposable> AcquireAsync(string key, CancellationToken ct = default)
    {
        var semaphore = GetLock(key);
        await semaphore.WaitAsync(ct).ConfigureAwait(false);
        return new SemaphoreReleaser(semaphore);
    }

    /// <summary>
    /// Tries to acquire a lock asynchronously with a timeout.
    /// </summary>
    /// <param name="key">The key to lock on.</param>
    /// <param name="timeout">The maximum time to wait for the lock.</param>
    /// <param name="ct">Cancellation token to stop the wait operation.</param>
    /// <returns>A tuple containing a boolean indicating success and a disposable lock if successful.</returns>
    public static async Task<(bool Acquired, IDisposable? Lock)> TryAcquireAsync(string key, TimeSpan timeout, CancellationToken ct = default)
    {
        var semaphore = GetLock(key);
        bool acquired = await semaphore.WaitAsync(timeout, ct).ConfigureAwait(false);
        return (acquired, acquired ? new SemaphoreReleaser(semaphore) : null);
    }

    /// <summary>
    /// Removes unused locks to free up resources.
    /// </summary>
    public static void CleanupUnusedLocks()
    {
        var keysToRemove = _locks.Where(kvp => kvp.Value.CurrentCount == 1).Select(kvp => kvp.Key).ToList();
        foreach (var key in keysToRemove)
        {
            if (_locks.TryRemove(key, out var semaphore))
            {
                semaphore.Dispose();
            }
        }
    }

    /// <summary>
    /// A disposable class that releases a semaphore when disposed.
    /// </summary>
    private class SemaphoreReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemaphoreReleaser"/> class.
        /// </summary>
        /// <param name="semaphore">The semaphore to release when disposed.</param>
        public SemaphoreReleaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        /// <summary>
        /// Releases the semaphore.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _semaphore.Release();
            _disposed = true;
        }
    }
}