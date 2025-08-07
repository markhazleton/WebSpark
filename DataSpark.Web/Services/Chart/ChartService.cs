using DataSpark.Web.Models.Chart;
using DataSpark.Web.Services.Chart;

namespace DataSpark.Web.Services.Chart;

/// <summary>
/// Implementation of chart configuration service
/// </summary>
public class ChartService : IChartService
{
    private readonly IChartConfigurationRepository _repository;
    private readonly IChartValidationService _validationService;
    private readonly ILogger<ChartService> _logger;

    public ChartService(
        IChartConfigurationRepository repository,
        IChartValidationService validationService,
        ILogger<ChartService> logger)
    {
        _repository = repository;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<ChartConfiguration?> GetConfigurationAsync(int id)
    {
        try
        {
            return await _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chart configuration {Id}", id);
            return null;
        }
    }

    public async Task<ChartConfiguration> SaveConfigurationAsync(ChartConfiguration config)
    {
        try
        {
            _logger.LogInformation("Starting save operation for chart configuration: {Name}, DataSource: {DataSource}, SeriesCount: {SeriesCount}",
                config.Name, config.CsvFile, config.Series?.Count ?? 0);

            // Validate configuration
            var validationResult = await _validationService.ValidateConfigurationAsync(config);
            if (!validationResult.IsValid)
            {
                var errorMessage = $"Configuration validation failed: {string.Join(", ", validationResult.Errors)}";
                _logger.LogWarning("Chart validation failed for '{Name}': {Errors}", config.Name, string.Join("; ", validationResult.Errors));
                throw new InvalidOperationException(errorMessage);
            }

            // Check for duplicate names
            var existingConfig = await _repository.GetByNameAsync(config.Name, config.CsvFile);
            if (existingConfig != null && existingConfig.Id != config.Id)
            {
                var errorMessage = $"A chart named '{config.Name}' already exists for this data source. Please choose a different name.";
                _logger.LogWarning("Duplicate chart name detected: {Name} for data source: {DataSource}", config.Name, config.CsvFile);
                throw new InvalidOperationException(errorMessage);
            }

            // Additional validation logging
            _logger.LogDebug("Chart configuration details - Type: {ChartType}, Style: {ChartStyle}, Dimensions: {Width}x{Height}",
                config.ChartType, config.ChartStyle, config.Width, config.Height);

            if (config.Series != null)
            {
                for (int i = 0; i < config.Series.Count; i++)
                {
                    var series = config.Series[i];
                    _logger.LogDebug("Series {Index}: Name='{Name}', Column='{Column}', Aggregation='{Aggregation}'",
                        i + 1, series.Name, series.DataColumn, series.AggregationFunction);
                }
            }

            ChartConfiguration savedConfig;
            if (config.Id == 0)
            {
                _logger.LogInformation("Creating new chart configuration: {Name}", config.Name);
                savedConfig = await _repository.CreateAsync(config);
                _logger.LogInformation("Successfully created chart configuration: {Name} with ID {Id}", config.Name, savedConfig.Id);
            }
            else
            {
                _logger.LogInformation("Updating existing chart configuration: {Name} (ID: {Id})", config.Name, config.Id);
                savedConfig = await _repository.UpdateAsync(config);
                _logger.LogInformation("Successfully updated chart configuration: {Name} (ID: {Id})", config.Name, savedConfig.Id);
            }

            return savedConfig;
        }
        catch (InvalidOperationException)
        {
            // Re-throw validation errors as-is (they already have user-friendly messages)
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving chart configuration: {Name}. Error: {Message}", config.Name, ex.Message);
            throw new InvalidOperationException($"Failed to save chart configuration '{config.Name}'. Please check that all required fields are filled and try again.", ex);
        }
    }

    public async Task<bool> DeleteConfigurationAsync(int id)
    {
        try
        {
            var result = await _repository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Deleted chart configuration {Id}", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chart configuration {Id}", id);
            return false;
        }
    }

    public async Task<List<ChartConfigurationSummary>> GetConfigurationsAsync(string? dataSource = null)
    {
        try
        {
            return await _repository.GetSummariesAsync(dataSource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chart configurations for data source: {DataSource}", dataSource);
            return new List<ChartConfigurationSummary>();
        }
    }

    public async Task<ChartConfiguration?> GetConfigurationByNameAsync(string name, string dataSource)
    {
        try
        {
            return await _repository.GetByNameAsync(name, dataSource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chart configuration by name: {Name}", name);
            return null;
        }
    }

    public async Task<bool> ConfigurationExistsAsync(string name, string dataSource, int? excludeId = null)
    {
        try
        {
            return await _repository.ExistsByNameAsync(name, dataSource, excludeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if configuration exists: {Name}", name);
            return false;
        }
    }

    public async Task<ChartConfiguration> DuplicateConfigurationAsync(int id, string newName)
    {
        try
        {
            var originalConfig = await _repository.GetByIdAsync(id);
            if (originalConfig == null)
            {
                throw new ArgumentException($"Configuration with ID {id} not found");
            }

            // Create a copy
            var duplicatedConfig = originalConfig.Clone();
            duplicatedConfig.Name = newName;

            // Check for name conflicts
            if (await _repository.ExistsByNameAsync(newName, duplicatedConfig.CsvFile))
            {
                throw new InvalidOperationException($"A configuration with the name '{newName}' already exists");
            }

            var savedConfig = await _repository.CreateAsync(duplicatedConfig);
            _logger.LogInformation("Duplicated chart configuration {OriginalId} as {NewName}", id, newName);

            return savedConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating chart configuration {Id}", id);
            throw;
        }
    }

    public async Task<List<ChartConfiguration>> GetConfigurationsByIdsAsync(List<int> ids)
    {
        try
        {
            return await _repository.GetByIdsAsync(ids);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving multiple chart configurations");
            return new List<ChartConfiguration>();
        }
    }

    public async Task<int> DeleteConfigurationsAsync(List<int> ids)
    {
        try
        {
            var deletedCount = await _repository.DeleteByIdsAsync(ids);
            _logger.LogInformation("Deleted {Count} chart configurations", deletedCount);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple chart configurations");
            return 0;
        }
    }
}
