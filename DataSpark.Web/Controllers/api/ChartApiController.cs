using Microsoft.AspNetCore.Mvc;
using DataSpark.Web.Models;
using DataSpark.Web.Models.Chart;
using DataSpark.Web.Services.Chart;

namespace DataSpark.Web.Controllers.Api;

/// <summary>
/// RESTful API controller for chart operations
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ChartApiController : ControllerBase
{
    private readonly IChartService _chartService;
    private readonly IDataService _dataService;
    private readonly IChartRenderingService _renderingService;
    private readonly IChartValidationService _validationService;
    private readonly ILogger<ChartApiController> _logger;

    public ChartApiController(
        IChartService chartService,
        IDataService dataService,
        IChartRenderingService renderingService,
        IChartValidationService validationService,
        ILogger<ChartApiController> logger)
    {
        _chartService = chartService;
        _dataService = dataService;
        _renderingService = renderingService;
        _validationService = validationService;
        _logger = logger;
    }

    /// <summary>
    /// Get chart data for a specific data source
    /// </summary>
    [HttpGet("data/{dataSource}")]
    public async Task<ActionResult<ApiResponse<ProcessedChartData>>> GetChartData(
        string dataSource,
        [FromQuery] ChartDataRequest request)
    {
        try
        {
            // Create a temporary configuration from the request
            var tempConfig = new ChartConfiguration
            {
                CsvFile = dataSource,
                ChartType = "Column", // Default
                Series = request.YColumns.Select((col, index) => new ChartSeries
                {
                    Name = col,
                    DataColumn = col,
                    AggregationFunction = request.AggregationFunction,
                    DisplayOrder = index,
                    IsVisible = true
                }).ToList(),
                XAxis = !string.IsNullOrWhiteSpace(request.XColumn)
                    ? new ChartAxis { AxisType = "X", DataColumn = request.XColumn }
                    : null,
                Filters = request.Filters.Select(f => new ChartFilter
                {
                    Column = f.Column,
                    FilterType = f.FilterType,
                    IncludedValues = f.Values,
                    MinValue = f.MinValue,
                    MaxValue = f.MaxValue,
                    Pattern = f.Pattern,
                    IsCaseSensitive = f.IsCaseSensitive,
                    IsRegex = f.IsRegex,
                    IsEnabled = true
                }).ToList()
            };

            var processedData = await _dataService.ProcessDataAsync(dataSource, tempConfig);

            // Apply limit and offset if specified
            if (request.Limit.HasValue || request.Offset.HasValue)
            {
                var skip = request.Offset ?? 0;
                var take = request.Limit ?? processedData.DataPoints.Count;

                processedData.DataPoints = processedData.DataPoints
                    .Skip(skip)
                    .Take(take)
                    .ToList();
            }

            return ApiResponse<ProcessedChartData>.SuccessResult(processedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chart data for {DataSource}", dataSource);
            return ApiResponse<ProcessedChartData>.ErrorResult("Error processing chart data");
        }
    }

    /// <summary>
    /// Get columns for a data source
    /// </summary>
    [HttpGet("columns/{dataSource}")]
    public async Task<ActionResult<ApiResponse<List<ColumnInfo>>>> GetColumns(string dataSource)
    {
        try
        {
            var columns = await _dataService.GetColumnsAsync(dataSource);
            return ApiResponse<List<ColumnInfo>>.SuccessResult(columns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting columns for {DataSource}", dataSource);
            return ApiResponse<List<ColumnInfo>>.ErrorResult("Error retrieving column information");
        }
    }

    /// <summary>
    /// Get unique values for a specific column
    /// </summary>
    [HttpGet("values/{dataSource}/{column}")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetColumnValues(
        string dataSource,
        string column,
        [FromQuery] int maxValues = 1000)
    {
        try
        {
            var values = await _dataService.GetColumnValuesAsync(dataSource, column, maxValues);
            return ApiResponse<List<string>>.SuccessResult(values);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting values for column {Column} in {DataSource}", column, dataSource);
            return ApiResponse<List<string>>.ErrorResult("Error retrieving column values");
        }
    }

    /// <summary>
    /// Get values for multiple columns
    /// </summary>
    [HttpPost("values/{dataSource}")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, List<string>>>>> GetMultipleColumnValues(
        string dataSource,
        [FromBody] List<string> columns,
        [FromQuery] int maxValues = 100)
    {
        try
        {
            var values = await _dataService.GetMultipleColumnValuesAsync(dataSource, columns, maxValues);
            return ApiResponse<Dictionary<string, List<string>>>.SuccessResult(values);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple column values for {DataSource}", dataSource);
            return ApiResponse<Dictionary<string, List<string>>>.ErrorResult("Error retrieving column values");
        }
    }

    /// <summary>
    /// Render a chart configuration
    /// </summary>
    [HttpPost("render")]
    public async Task<ActionResult<ApiResponse<ChartRenderResult>>> RenderChart([FromBody] ChartConfiguration config)
    {
        try
        {
            // Validate the configuration
            var validationResult = await _validationService.ValidateConfigurationAsync(config, config.CsvFile);
            if (!validationResult.IsValid)
            {
                return ApiResponse<ChartRenderResult>.ErrorResult(
                    "Configuration validation failed",
                    validationResult.Errors);
            }

            // Process the data
            var processedData = await _dataService.ProcessDataAsync(config.CsvFile, config);

            // Render the chart
            var renderResult = await _renderingService.RenderChartAsync(config, processedData);

            if (!renderResult.Success)
            {
                return ApiResponse<ChartRenderResult>.ErrorResult(
                    "Chart rendering failed",
                    renderResult.Errors);
            }

            return ApiResponse<ChartRenderResult>.SuccessResult(renderResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering chart");
            return ApiResponse<ChartRenderResult>.ErrorResult("Error rendering chart");
        }
    }

    /// <summary>
    /// Validate a chart configuration
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<ValidationResult>>> ValidateConfiguration([FromBody] ValidationRequest request)
    {
        try
        {
            var validationResult = await _validationService.ValidateConfigurationAsync(
                request.Configuration,
                request.DataSource);

            return ApiResponse<ValidationResult>.SuccessResult(validationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating chart configuration");
            return ApiResponse<ValidationResult>.ErrorResult("Error validating configuration");
        }
    }

    /// <summary>
    /// Get data summary for a data source
    /// </summary>
    [HttpGet("summary/{dataSource}")]
    public async Task<ActionResult<ApiResponse<DataSummary>>> GetDataSummary(string dataSource)
    {
        try
        {
            var summary = await _dataService.GetDataSummaryAsync(dataSource);
            return ApiResponse<DataSummary>.SuccessResult(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data summary for {DataSource}", dataSource);
            return ApiResponse<DataSummary>.ErrorResult("Error retrieving data summary");
        }
    }

    /// <summary>
    /// Get available data sources
    /// </summary>
    [HttpGet("datasources")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetDataSources()
    {
        try
        {
            var dataSources = await _dataService.GetAvailableDataSourcesAsync();
            return ApiResponse<List<string>>.SuccessResult(dataSources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available data sources");
            return ApiResponse<List<string>>.ErrorResult("Error retrieving data sources");
        }
    }

    /// <summary>
    /// Get chart configurations
    /// </summary>
    [HttpGet("configurations")]
    public async Task<ActionResult<ApiResponse<List<ChartConfigurationSummary>>>> GetConfigurations(
        [FromQuery] string? dataSource = null)
    {
        try
        {
            var configurations = await _chartService.GetConfigurationsAsync(dataSource);
            return ApiResponse<List<ChartConfigurationSummary>>.SuccessResult(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chart configurations");
            return ApiResponse<List<ChartConfigurationSummary>>.ErrorResult("Error retrieving configurations");
        }
    }

    /// <summary>
    /// Get a specific chart configuration
    /// </summary>
    [HttpGet("configurations/{id}")]
    public async Task<ActionResult<ApiResponse<ChartConfiguration>>> GetConfiguration(int id)
    {
        try
        {
            var configuration = await _chartService.GetConfigurationAsync(id);
            if (configuration == null)
            {
                return NotFound(ApiResponse<ChartConfiguration>.ErrorResult("Configuration not found"));
            }

            return ApiResponse<ChartConfiguration>.SuccessResult(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chart configuration {Id}", id);
            return ApiResponse<ChartConfiguration>.ErrorResult("Error retrieving configuration");
        }
    }

    /// <summary>
    /// Save a chart configuration
    /// </summary>
    [HttpPost("configurations")]
    public async Task<ActionResult<ApiResponse<ChartConfiguration>>> SaveConfiguration([FromBody] ChartConfiguration config)
    {
        try
        {
            var savedConfig = await _chartService.SaveConfigurationAsync(config);
            return ApiResponse<ChartConfiguration>.SuccessResult(savedConfig, "Configuration saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving chart configuration");
            return ApiResponse<ChartConfiguration>.ErrorResult(ex.Message);
        }
    }

    /// <summary>
    /// Delete a chart configuration
    /// </summary>
    [HttpDelete("configurations/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteConfiguration(int id)
    {
        try
        {
            var result = await _chartService.DeleteConfigurationAsync(id);
            if (result)
            {
                return ApiResponse<bool>.SuccessResult(true, "Configuration deleted successfully");
            }
            else
            {
                return ApiResponse<bool>.ErrorResult("Configuration not found or could not be deleted");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chart configuration {Id}", id);
            return ApiResponse<bool>.ErrorResult("Error deleting configuration");
        }
    }

    /// <summary>
    /// Get available chart types
    /// </summary>
    [HttpGet("charttypes")]
    public ActionResult<ApiResponse<Dictionary<string, ChartTypeInfo>>> GetChartTypes()
    {
        try
        {
            return ApiResponse<Dictionary<string, ChartTypeInfo>>.SuccessResult(ChartTypes.All);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chart types");
            return ApiResponse<Dictionary<string, ChartTypeInfo>>.ErrorResult("Error retrieving chart types");
        }
    }

    /// <summary>
    /// Get available color palettes
    /// </summary>
    [HttpGet("palettes")]
    public ActionResult<ApiResponse<Dictionary<string, string[]>>> GetColorPalettes()
    {
        try
        {
            return ApiResponse<Dictionary<string, string[]>>.SuccessResult(ColorPalettes.All);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting color palettes");
            return ApiResponse<Dictionary<string, string[]>>.ErrorResult("Error retrieving color palettes");
        }
    }

    /// <summary>
    /// Bulk operations on configurations
    /// </summary>
    [HttpPost("configurations/bulk")]
    public async Task<ActionResult<ApiResponse<object>>> BulkOperation([FromBody] BulkOperationRequest request)
    {
        try
        {
            switch (request.Operation.ToLowerInvariant())
            {
                case "delete":
                    var deletedCount = await _chartService.DeleteConfigurationsAsync(request.ConfigurationIds);
                    return ApiResponse<object>.SuccessResult(
                        new { deletedCount },
                        $"Deleted {deletedCount} configurations");

                default:
                    return ApiResponse<object>.ErrorResult("Unknown bulk operation");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation: {Operation}", request.Operation);
            return ApiResponse<object>.ErrorResult("Error performing bulk operation");
        }
    }
}
