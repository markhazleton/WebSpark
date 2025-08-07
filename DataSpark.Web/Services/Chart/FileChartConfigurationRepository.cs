using DataSpark.Web.Models.Chart;
using DataSpark.Web.Services.Chart;
using System.Text.Json;
using System.Collections.Concurrent;

namespace DataSpark.Web.Services.Chart;

/// <summary>
/// File-based implementation of chart configuration repository
/// Saves configurations to JSON files in the data/charts folder
/// </summary>
public class FileChartConfigurationRepository : IChartConfigurationRepository
{
    private readonly string _dataFolderPath;
    private readonly string _chartsFolderPath;
    private readonly ILogger<FileChartConfigurationRepository> _logger;
    private readonly object _lockObject = new();
    private readonly ConcurrentDictionary<int, string> _filePathCache = new();
    private int _nextId = 1;

    public FileChartConfigurationRepository(IWebHostEnvironment env, ILogger<FileChartConfigurationRepository> logger)
    {
        _logger = logger;
        _dataFolderPath = Path.Combine(env.ContentRootPath, "data");
        _chartsFolderPath = Path.Combine(_dataFolderPath, "charts");

        // Ensure directories exist
        Directory.CreateDirectory(_dataFolderPath);
        Directory.CreateDirectory(_chartsFolderPath);

        // Initialize next ID based on existing files
        InitializeNextId();

        _logger.LogInformation("FileChartConfigurationRepository initialized. Charts folder: {ChartsFolder}", _chartsFolderPath);
    }

    private void InitializeNextId()
    {
        try
        {
            var files = Directory.GetFiles(_chartsFolderPath, "chart_*.json");
            if (files.Length > 0)
            {
                var maxId = 0;
                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (fileName.StartsWith("chart_") && int.TryParse(fileName.Substring(6), out var id))
                    {
                        if (id > maxId) maxId = id;
                        _filePathCache[id] = file;
                    }
                }
                _nextId = maxId + 1;
            }

            _logger.LogInformation("Initialized with {FileCount} existing charts. Next ID: {NextId}", files.Length, _nextId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing next ID from existing files");
            _nextId = 1;
        }
    }

    public async Task<ChartConfiguration?> GetByIdAsync(int id)
    {
        try
        {
            var filePath = GetFilePath(id);
            if (!File.Exists(filePath))
            {
                _logger.LogDebug("Chart configuration file not found: {FilePath}", filePath);
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var config = JsonSerializer.Deserialize<ChartConfiguration>(json, GetJsonOptions());

            _logger.LogDebug("Successfully loaded chart configuration {Id} from file", id);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading chart configuration {Id}", id);
            return null;
        }
    }

    public async Task<ChartConfiguration?> GetByNameAsync(string name, string dataSource)
    {
        try
        {
            var configs = await GetAllAsync();
            var config = configs.FirstOrDefault(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                c.CsvFile.Equals(dataSource, StringComparison.OrdinalIgnoreCase));

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding chart configuration by name: {Name}", name);
            return null;
        }
    }

    public async Task<List<ChartConfiguration>> GetByDataSourceAsync(string dataSource)
    {
        try
        {
            var configs = await GetAllAsync();
            var filtered = configs
                .Where(c => c.CsvFile.Equals(dataSource, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.Name)
                .ToList();

            _logger.LogDebug("Found {Count} charts for data source: {DataSource}", filtered.Count, dataSource);
            return filtered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving charts by data source: {DataSource}", dataSource);
            return new List<ChartConfiguration>();
        }
    }

    public async Task<List<ChartConfiguration>> GetAllAsync()
    {
        try
        {
            var configs = new List<ChartConfiguration>();
            var files = Directory.GetFiles(_chartsFolderPath, "chart_*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var config = JsonSerializer.Deserialize<ChartConfiguration>(json, GetJsonOptions());
                    if (config != null)
                    {
                        configs.Add(config);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error loading chart configuration from file: {File}", file);
                }
            }

            var sorted = configs.OrderBy(c => c.Name).ToList();
            _logger.LogDebug("Loaded {Count} chart configurations from files", sorted.Count);
            return sorted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all chart configurations");
            return new List<ChartConfiguration>();
        }
    }

    public async Task<ChartConfiguration> CreateAsync(ChartConfiguration config)
    {
        try
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
            }

            // Save to file
            var filePath = GetFilePath(config.Id);
            var json = JsonSerializer.Serialize(config, GetJsonOptions());
            await File.WriteAllTextAsync(filePath, json);

            _filePathCache[config.Id] = filePath;

            _logger.LogInformation("Successfully created chart configuration {Id} '{Name}' in file: {FilePath}",
                config.Id, config.Name, filePath);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chart configuration: {Name}", config.Name);
            throw;
        }
    }

    public async Task<ChartConfiguration> UpdateAsync(ChartConfiguration config)
    {
        try
        {
            if (config.Id <= 0)
            {
                throw new ArgumentException("Configuration ID must be greater than 0 for updates");
            }

            config.ModifiedDate = DateTime.UtcNow;

            var filePath = GetFilePath(config.Id);
            var json = JsonSerializer.Serialize(config, GetJsonOptions());
            await File.WriteAllTextAsync(filePath, json);

            _logger.LogInformation("Successfully updated chart configuration {Id} '{Name}' in file: {FilePath}",
                config.Id, config.Name, filePath);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chart configuration {Id}: {Name}", config.Id, config.Name);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var filePath = GetFilePath(id);
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Cannot delete chart configuration {Id}: file not found at {FilePath}", id, filePath);
                return false;
            }

            File.Delete(filePath);
            _filePathCache.TryRemove(id, out _);

            _logger.LogInformation("Successfully deleted chart configuration {Id} from file: {FilePath}", id, filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chart configuration {Id}", id);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            var filePath = GetFilePath(id);
            return File.Exists(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if chart configuration {Id} exists", id);
            return false;
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, string dataSource, int? excludeId = null)
    {
        try
        {
            var configs = await GetAllAsync();
            var exists = configs.Any(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                c.CsvFile.Equals(dataSource, StringComparison.OrdinalIgnoreCase) &&
                (excludeId == null || c.Id != excludeId));

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if chart configuration exists by name: {Name}", name);
            return false;
        }
    }

    public async Task<List<ChartConfiguration>> GetByIdsAsync(List<int> ids)
    {
        try
        {
            var configs = new List<ChartConfiguration>();

            foreach (var id in ids)
            {
                var config = await GetByIdAsync(id);
                if (config != null)
                {
                    configs.Add(config);
                }
            }

            return configs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chart configurations by IDs");
            return new List<ChartConfiguration>();
        }
    }

    public async Task<int> DeleteByIdsAsync(List<int> ids)
    {
        try
        {
            var deletedCount = 0;

            foreach (var id in ids)
            {
                if (await DeleteAsync(id))
                {
                    deletedCount++;
                }
            }

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chart configurations by IDs");
            return 0;
        }
    }

    public async Task<List<ChartConfigurationSummary>> GetSummariesAsync(string? dataSource = null)
    {
        try
        {
            var configs = await GetAllAsync();
            var query = configs.AsEnumerable();

            _logger.LogDebug("FileRepository GetSummariesAsync: Total configs loaded: {Count}, DataSource filter: '{DataSource}'",
                configs.Count, dataSource ?? "NULL");

            if (!string.IsNullOrWhiteSpace(dataSource))
            {
                // Filter by specific data source
                var beforeCount = query.Count();
                query = query.Where(c => c.CsvFile.Equals(dataSource, StringComparison.OrdinalIgnoreCase));
                var afterCount = query.Count();

                _logger.LogDebug("FileRepository: Filtered from {BeforeCount} to {AfterCount} configs for data source '{DataSource}'",
                    beforeCount, afterCount, dataSource);
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

            _logger.LogDebug("FileRepository: Returning {Count} chart summaries", summaries.Count);
            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chart configuration summaries");
            return new List<ChartConfigurationSummary>();
        }
    }

    private string GetFilePath(int id)
    {
        if (_filePathCache.TryGetValue(id, out var cachedPath) && File.Exists(cachedPath))
        {
            return cachedPath;
        }

        var filePath = Path.Combine(_chartsFolderPath, $"chart_{id}.json");
        _filePathCache[id] = filePath;
        return filePath;
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }
}
