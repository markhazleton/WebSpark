using DataSpark.Web.Models.Chart;
using DataSpark.Web.Services.Chart;
using System.Collections.Concurrent;

namespace DataSpark.Web.Services.Chart;

/// <summary>
/// In-memory implementation of chart configuration repository
/// </summary>
public class InMemoryChartConfigurationRepository : IChartConfigurationRepository
{
    private readonly ConcurrentDictionary<int, ChartConfiguration> _configurations = new();
    private int _nextId = 1;
    private readonly object _lockObject = new();

    public Task<ChartConfiguration?> GetByIdAsync(int id)
    {
        _configurations.TryGetValue(id, out var config);
        return Task.FromResult(config);
    }

    public Task<ChartConfiguration?> GetByNameAsync(string name, string dataSource)
    {
        var config = _configurations.Values
            .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                               c.CsvFile.Equals(dataSource, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(config);
    }

    public Task<List<ChartConfiguration>> GetByDataSourceAsync(string dataSource)
    {
        var configs = _configurations.Values
            .Where(c => c.CsvFile.Equals(dataSource, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Name)
            .ToList();
        return Task.FromResult(configs);
    }

    public Task<List<ChartConfiguration>> GetAllAsync()
    {
        var configs = _configurations.Values.OrderBy(c => c.Name).ToList();
        return Task.FromResult(configs);
    }

    public Task<ChartConfiguration> CreateAsync(ChartConfiguration config)
    {
        lock (_lockObject)
        {
            config.Id = _nextId++;
            config.CreatedDate = DateTime.UtcNow;
            config.ModifiedDate = DateTime.UtcNow;

            // Assign IDs to related entities
            foreach (var series in config.Series)
            {
                series.Id = _nextId++;
                series.ChartConfigurationId = config.Id;
            }

            if (config.XAxis != null)
            {
                config.XAxis.Id = _nextId++;
                config.XAxis.ChartConfigurationId = config.Id;
            }

            if (config.YAxis != null)
            {
                config.YAxis.Id = _nextId++;
                config.YAxis.ChartConfigurationId = config.Id;
            }

            if (config.Y2Axis != null)
            {
                config.Y2Axis.Id = _nextId++;
                config.Y2Axis.ChartConfigurationId = config.Id;
            }

            foreach (var filter in config.Filters)
            {
                filter.Id = _nextId++;
                filter.ChartConfigurationId = config.Id;
            }

            _configurations[config.Id] = config;
        }

        return Task.FromResult(config);
    }

    public Task<ChartConfiguration> UpdateAsync(ChartConfiguration config)
    {
        if (_configurations.ContainsKey(config.Id))
        {
            config.ModifiedDate = DateTime.UtcNow;
            _configurations[config.Id] = config;
        }

        return Task.FromResult(config);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var result = _configurations.TryRemove(id, out _);
        return Task.FromResult(result);
    }

    public Task<bool> ExistsAsync(int id)
    {
        var exists = _configurations.ContainsKey(id);
        return Task.FromResult(exists);
    }

    public Task<bool> ExistsByNameAsync(string name, string dataSource, int? excludeId = null)
    {
        var exists = _configurations.Values.Any(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
            c.CsvFile.Equals(dataSource, StringComparison.OrdinalIgnoreCase) &&
            (excludeId == null || c.Id != excludeId));

        return Task.FromResult(exists);
    }

    public Task<List<ChartConfiguration>> GetByIdsAsync(List<int> ids)
    {
        var configs = ids.Select(id => _configurations.TryGetValue(id, out var config) ? config : null)
                         .Where(c => c != null)
                         .Cast<ChartConfiguration>()
                         .ToList();

        return Task.FromResult(configs);
    }

    public Task<int> DeleteByIdsAsync(List<int> ids)
    {
        var deletedCount = 0;
        foreach (var id in ids)
        {
            if (_configurations.TryRemove(id, out _))
                deletedCount++;
        }

        return Task.FromResult(deletedCount);
    }

    public Task<List<ChartConfigurationSummary>> GetSummariesAsync(string? dataSource = null)
    {
        var query = _configurations.Values.AsEnumerable();

        // Debug logging to see what's in the repository
        Console.WriteLine($"=== REPOSITORY DEBUG ===");
        Console.WriteLine($"Total configurations in repository: {_configurations.Count}");
        foreach (var config in _configurations.Values)
        {
            Console.WriteLine($"Config ID: {config.Id}, Name: '{config.Name}', CsvFile: '{config.CsvFile}'");
        }
        Console.WriteLine($"Requested dataSource: '{dataSource ?? "NULL"}'");

        if (!string.IsNullOrWhiteSpace(dataSource))
        {
            // Filter by specific data source
            query = query.Where(c => c.CsvFile.Equals(dataSource, StringComparison.OrdinalIgnoreCase));
            Console.WriteLine($"After filtering by '{dataSource}': {query.Count()} configurations");
        }
        // If dataSource is null, return all charts (no filtering)

        var summaries = query.Select(c => new ChartConfigurationSummary
        {
            Id = c.Id,
            Name = c.Name,
            CsvFile = c.CsvFile,
            ChartType = c.ChartType,
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate,
            CreatedBy = c.CreatedBy,
            SeriesCount = c.Series.Count,
            FilterCount = c.Filters.Count,
            Description = $"{c.ChartType} chart with {c.Series.Count} series"
        })
        .OrderBy(s => s.Name)
        .ToList();

        Console.WriteLine($"Final summaries count: {summaries.Count}");
        Console.WriteLine($"=== END REPOSITORY DEBUG ===");

        return Task.FromResult(summaries);
    }
}
