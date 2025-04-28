using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace HttpClientCrawler.Crawler;

/// <summary>
/// Tracks performance metrics for the crawler operations.
/// </summary>
/// <remarks>
/// Use this class to collect performance data about crawler operations
/// for analysis and optimization. Helps identify bottlenecks in the crawling process.
/// </remarks>
public class CrawlerPerformanceTracker
{
    private readonly ConcurrentDictionary<string, (int Count, long TotalMilliseconds)> _metrics = new();
    private readonly ILogger _logger;
    private readonly Stopwatch _globalStopwatch;
    private readonly string _crawlId;

    /// <summary>
    /// Initializes a new instance of the <see cref="CrawlerPerformanceTracker"/> class.
    /// </summary>
    /// <param name="logger">The logger for performance information.</param>
    public CrawlerPerformanceTracker(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _crawlId = Guid.NewGuid().ToString("N").Substring(0, 8);
        _globalStopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Tracks the execution time of an operation.
    /// </summary>
    /// <param name="operation">The name of the operation being tracked.</param>
    /// <param name="milliseconds">The elapsed time in milliseconds.</param>
    public void TrackOperation(string operation, long milliseconds)
    {
        _metrics.AddOrUpdate(
            operation,
            (1, milliseconds),
            (_, existing) => (existing.Count + 1, existing.TotalMilliseconds + milliseconds)
        );
    }

    /// <summary>
    /// Executes and tracks the execution time of an operation.
    /// </summary>
    /// <param name="operation">The name of the operation being tracked.</param>
    /// <param name="action">The action to execute.</param>
    public void ExecuteAndTrack(string operation, Action action)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            action();
        }
        finally
        {
            sw.Stop();
            TrackOperation(operation, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Executes and tracks the execution time of an asynchronous operation.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="operation">The name of the operation being tracked.</param>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="ct">Cancellation token to propagate cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    public async Task<T> ExecuteAndTrackAsync<T>(string operation, Func<Task<T>> func, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            ct.ThrowIfCancellationRequested();
            return await func().ConfigureAwait(false);
        }
        finally
        {
            sw.Stop();
            TrackOperation(operation, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Executes and tracks the execution time of an asynchronous operation with no result.
    /// </summary>
    /// <param name="operation">The name of the operation being tracked.</param>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="ct">Cancellation token to propagate cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecuteAndTrackAsync(string operation, Func<Task> func, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            ct.ThrowIfCancellationRequested();
            await func().ConfigureAwait(false);
        }
        finally
        {
            sw.Stop();
            TrackOperation(operation, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Estimates memory requirements for a crawl operation.
    /// </summary>
    /// <param name="maxPages">The maximum number of pages to be crawled.</param>
    /// <param name="estimatedMemoryPerPageKb">The estimated memory per page in kilobytes.</param>
    /// <returns>The estimated memory in megabytes.</returns>
    public double EstimateMemoryRequirements(int maxPages, int estimatedMemoryPerPageKb = 100)
    {
        double estimatedMemoryMb = (maxPages * estimatedMemoryPerPageKb) / 1024.0;
        _logger.LogInformation(
            "Crawl {CrawlId}: Estimated memory for {MaxPages} pages: {MemoryMB:F2} MB",
            _crawlId, maxPages, estimatedMemoryMb);

        return estimatedMemoryMb;
    }

    /// <summary>
    /// Logs the performance metrics collected.
    /// </summary>
    public void LogMetrics()
    {
        _globalStopwatch.Stop();
        _logger.LogInformation("Crawl {CrawlId}: Total execution time: {TotalTime:F2} seconds",
            _crawlId, _globalStopwatch.ElapsedMilliseconds / 1000.0);

        foreach (var metric in _metrics)
        {
            _logger.LogInformation(
                "Crawl {CrawlId}: Operation '{Operation}': {Count} calls, {TotalTime:F2} ms total, {AvgTime:F2} ms average",
                _crawlId,
                metric.Key,
                metric.Value.Count,
                metric.Value.TotalMilliseconds,
                metric.Value.TotalMilliseconds / metric.Value.Count);
        }
    }
}