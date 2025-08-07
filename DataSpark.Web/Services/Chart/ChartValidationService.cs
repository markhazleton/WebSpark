using DataSpark.Web.Models;
using DataSpark.Web.Models.Chart;
using DataSpark.Web.Services.Chart;
using System.Text.Json;

namespace DataSpark.Web.Services.Chart;

/// <summary>
/// Implementation of chart validation service
/// </summary>
public class ChartValidationService : IChartValidationService
{
    private readonly IDataService _dataService;
    private readonly ILogger<ChartValidationService> _logger;

    public ChartValidationService(IDataService dataService, ILogger<ChartValidationService> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateConfigurationAsync(ChartConfiguration config, string? dataSource = null)
    {
        var result = new ValidationResult();

        try
        {
            _logger.LogInformation("Starting validation for chart configuration: {Name}, DataSource: {DataSource}",
                config.Name, dataSource ?? "Not specified");

            // Basic configuration validation
            var basicValidation = config.Validate();
            result.Errors.AddRange(basicValidation.Errors);
            result.Warnings.AddRange(basicValidation.Warnings);

            if (basicValidation.Errors.Any())
            {
                _logger.LogWarning("Basic validation failed for chart '{Name}': {Errors}",
                    config.Name, string.Join("; ", basicValidation.Errors));
            }

            // Data source validation
            if (!string.IsNullOrWhiteSpace(dataSource))
            {
                _logger.LogDebug("Validating data source: {DataSource}", dataSource);
                var isValidDataSource = await _dataService.ValidateDataSourceAsync(dataSource);
                if (!isValidDataSource)
                {
                    var errorMessage = $"The CSV file '{dataSource}' could not be found or accessed. Please ensure the file exists and try again.";
                    result.Errors.Add(errorMessage);
                    _logger.LogWarning("Data source validation failed: {DataSource}", dataSource);
                }
                else
                {
                    _logger.LogDebug("Data source validation passed for: {DataSource}", dataSource);

                    // Get columns for detailed validation
                    var columns = await _dataService.GetColumnsAsync(dataSource);
                    _logger.LogDebug("Found {ColumnCount} columns in data source: {Columns}",
                        columns.Count, string.Join(", ", columns.Select(c => c.Column)));

                    // Validate series
                    for (int i = 0; i < config.Series.Count; i++)
                    {
                        var series = config.Series[i];
                        _logger.LogDebug("Validating series {Index}: {Name} -> {Column}", i + 1, series.Name, series.DataColumn);
                        var seriesErrors = await ValidateSeriesAsync(series, columns);
                        if (seriesErrors.Any())
                        {
                            _logger.LogWarning("Series validation failed for '{SeriesName}': {Errors}",
                                series.Name, string.Join("; ", seriesErrors));
                        }
                        result.Errors.AddRange(seriesErrors);
                    }

                    // Validate axes
                    if (config.XAxis != null)
                    {
                        _logger.LogDebug("Validating X-axis: {Column}", config.XAxis.DataColumn);
                        var xAxisErrors = await ValidateAxisAsync(config.XAxis, columns);
                        if (xAxisErrors.Any())
                        {
                            _logger.LogWarning("X-axis validation failed: {Errors}", string.Join("; ", xAxisErrors));
                        }
                        result.Errors.AddRange(xAxisErrors);
                    }

                    if (config.YAxis != null)
                    {
                        _logger.LogDebug("Validating Y-axis: {Column}", config.YAxis.DataColumn);
                        var yAxisErrors = await ValidateAxisAsync(config.YAxis, columns);
                        if (yAxisErrors.Any())
                        {
                            _logger.LogWarning("Y-axis validation failed: {Errors}", string.Join("; ", yAxisErrors));
                        }
                        result.Errors.AddRange(yAxisErrors);
                    }

                    if (config.Y2Axis != null)
                    {
                        _logger.LogDebug("Validating Y2-axis: {Column}", config.Y2Axis.DataColumn);
                        var y2AxisErrors = await ValidateAxisAsync(config.Y2Axis, columns);
                        if (y2AxisErrors.Any())
                        {
                            _logger.LogWarning("Y2-axis validation failed: {Errors}", string.Join("; ", y2AxisErrors));
                        }
                        result.Errors.AddRange(y2AxisErrors);
                    }

                    // Validate filters
                    var filterErrors = await ValidateFiltersAsync(config.Filters, columns);
                    if (filterErrors.Any())
                    {
                        _logger.LogWarning("Filter validation failed: {Errors}", string.Join("; ", filterErrors));
                    }
                    result.Errors.AddRange(filterErrors);

                    // Chart type compatibility
                    var isCompatible = await IsChartTypeCompatibleAsync(config.ChartType, columns);
                    if (!isCompatible)
                    {
                        var warningMessage = $"The '{config.ChartType}' chart type may not be optimal for your data. Consider using a different chart type.";
                        result.Warnings.Add(warningMessage);
                        _logger.LogInformation("Chart type compatibility warning for '{ChartType}' with data source '{DataSource}'",
                            config.ChartType, dataSource);
                    }
                }
            }
            else
            {
                _logger.LogWarning("No data source specified for chart validation: {Name}", config.Name);
            }

            // Chart-specific validation
            ValidateChartSpecificRules(config, result);

            // Performance warnings
            AddPerformanceWarnings(config, result);

            _logger.LogInformation("Validation completed for chart '{Name}': {ErrorCount} errors, {WarningCount} warnings",
                config.Name, result.Errors.Count, result.Warnings.Count);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during chart validation for '{ConfigName}': {Message}",
                config.Name, ex.Message);
            result.Errors.Add("An unexpected error occurred during validation. Please check your configuration and try again.");
        }

        return result;
    }

    public async Task<List<string>> ValidateSeriesAsync(ChartSeries series, List<ColumnInfo> columns)
    {
        var errors = new List<string>();

        try
        {
            // Basic series validation
            var basicErrors = series.Validate();
            errors.AddRange(basicErrors);

            // Enhanced validation with user-friendly messages
            if (string.IsNullOrWhiteSpace(series.Name))
            {
                errors.Add("Series name is required. Please provide a name for this data series.");
            }

            if (string.IsNullOrWhiteSpace(series.DataColumn))
            {
                errors.Add($"Series '{series.Name}' requires a data column selection. Please choose a column from your CSV file.");
            }
            else
            {
                // Check if data column exists
                var dataColumn = columns.FirstOrDefault(c => c.Column.Equals(series.DataColumn, StringComparison.OrdinalIgnoreCase));
                if (dataColumn == null)
                {
                    var availableColumns = string.Join(", ", columns.Select(c => c.Column).Take(5));
                    errors.Add($"Series '{series.Name}': The column '{series.DataColumn}' was not found in your CSV file. Available columns include: {availableColumns}");
                }
                else
                {
                    // Check if column is appropriate for aggregation
                    if (!dataColumn.IsNumeric && !IsCountAggregation(series.AggregationFunction))
                    {
                        errors.Add($"Series '{series.Name}': The column '{series.DataColumn}' contains text data and can only use 'Count' aggregation. Please change the aggregation to Count or select a numeric column.");
                    }

                    // Validate chart type compatibility
                    if (!string.IsNullOrWhiteSpace(series.SeriesChartType))
                    {
                        var chartTypeInfo = ChartTypes.GetInfo(series.SeriesChartType);
                        if (chartTypeInfo == null)
                        {
                            errors.Add($"Series '{series.Name}': Invalid chart type '{series.SeriesChartType}'");
                        }
                    }
                }
            }

            // Validate color format
            if (!string.IsNullOrWhiteSpace(series.Color) && !IsValidColor(series.Color))
            {
                errors.Add($"Series '{series.Name}': Invalid color format '{series.Color}'");
            }

            // Validate marker and line settings
            if (series.MarkerSize < 1 || series.MarkerSize > 50)
            {
                errors.Add($"Series '{series.Name}': Marker size must be between 1 and 50");
            }

            if (series.LineWidth < 1 || series.LineWidth > 20)
            {
                errors.Add($"Series '{series.Name}': Line width must be between 1 and 20");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating series: {SeriesName}", series.Name);
            errors.Add($"Error validating series '{series.Name}'");
        }

        return errors;
    }

    public async Task<List<string>> ValidateAxisAsync(ChartAxis axis, List<ColumnInfo> columns)
    {
        var errors = new List<string>();

        try
        {
            // Basic axis validation
            var basicErrors = axis.Validate();
            errors.AddRange(basicErrors);

            // Check if data column exists (if specified)
            if (!string.IsNullOrWhiteSpace(axis.DataColumn))
            {
                var dataColumn = columns.FirstOrDefault(c => c.Column.Equals(axis.DataColumn, StringComparison.OrdinalIgnoreCase));
                if (dataColumn == null)
                {
                    errors.Add($"{axis.AxisType}-Axis: Data column '{axis.DataColumn}' not found");
                }
                else
                {
                    // Check data type compatibility with scale type
                    if (axis.ScaleType == "DateTime" && !IsDateTimeColumn(dataColumn))
                    {
                        errors.Add($"{axis.AxisType}-Axis: DateTime scale requires a date/time column");
                    }

                    if (axis.IsLogarithmic && !dataColumn.IsNumeric)
                    {
                        errors.Add($"{axis.AxisType}-Axis: Logarithmic scale requires a numeric column");
                    }
                }
            }

            // Validate selected values if any
            var selectedValues = axis.SelectedValues;
            if (selectedValues != null && selectedValues.Any() && !string.IsNullOrWhiteSpace(axis.DataColumn))
            {
                // Could validate against actual column values, but this might be expensive
                // Add as a warning instead
                if (selectedValues.Count > 100)
                {
                    errors.Add($"{axis.AxisType}-Axis: Too many selected values ({selectedValues.Count}). Consider using filters instead");
                }
            }

            // Validate color formats
            if (!IsValidColor(axis.GridLineColor))
            {
                errors.Add($"{axis.AxisType}-Axis: Invalid grid line color format");
            }

            if (!IsValidColor(axis.TickMarkColor))
            {
                errors.Add($"{axis.AxisType}-Axis: Invalid tick mark color format");
            }

            if (!IsValidColor(axis.LabelColor))
            {
                errors.Add($"{axis.AxisType}-Axis: Invalid label color format");
            }

            if (!IsValidColor(axis.TitleColor))
            {
                errors.Add($"{axis.AxisType}-Axis: Invalid title color format");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating axis: {AxisType}", axis.AxisType);
            errors.Add($"Error validating {axis.AxisType}-axis");
        }

        return errors;
    }

    public async Task<List<string>> ValidateFiltersAsync(List<ChartFilter> filters, List<ColumnInfo> columns)
    {
        var errors = new List<string>();

        try
        {
            foreach (var filter in filters)
            {
                // Basic filter validation
                var basicErrors = filter.Validate();
                errors.AddRange(basicErrors.Select(e => $"Filter '{filter.Column}': {e}"));

                // Check if column exists
                var column = columns.FirstOrDefault(c => c.Column.Equals(filter.Column, StringComparison.OrdinalIgnoreCase));
                if (column == null)
                {
                    errors.Add($"Filter: Column '{filter.Column}' not found");
                    continue;
                }

                // Validate range filters for numeric/date columns
                if (filter.FilterType == "Range")
                {
                    if (!column.IsNumeric && !IsDateTimeColumn(column))
                    {
                        errors.Add($"Filter '{filter.Column}': Range filters work best with numeric or date columns");
                    }
                }

                // Validate included/excluded values count
                var totalValues = filter.IncludedValues.Count + filter.ExcludedValues.Count;
                if (totalValues > 1000)
                {
                    errors.Add($"Filter '{filter.Column}': Too many filter values ({totalValues}). Consider using pattern filters instead");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating filters");
            errors.Add("Error validating filters");
        }

        return errors;
    }

    public async Task<bool> IsChartTypeCompatibleAsync(string chartType, List<ColumnInfo> columns)
    {
        try
        {
            var chartTypeInfo = ChartTypes.GetInfo(chartType);
            if (chartTypeInfo == null)
                return false;

            var numericColumns = columns.Count(c => c.IsNumeric);
            var dateColumns = columns.Count(c => IsDateTimeColumn(c));

            return chartType switch
            {
                "Pie" or "Doughnut" => numericColumns >= 1 && columns.Count >= 2,
                "Scatter" or "Bubble" => numericColumns >= 2,
                "Line" or "Area" or "Spline" or "SplineArea" => dateColumns >= 1 || columns.Any(c => c.IsCategory),
                _ => numericColumns >= 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking chart type compatibility: {ChartType}", chartType);
            return false;
        }
    }

    private void ValidateChartSpecificRules(ChartConfiguration config, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(config.ChartType))
        {
            result.Errors.Add("Chart type is required and cannot be empty");
            return;
        }

        var chartTypeInfo = ChartTypes.GetInfo(config.ChartType);
        if (chartTypeInfo == null)
        {
            result.Errors.Add($"Unknown chart type: {config.ChartType}");
            return;
        }

        // Pie/Doughnut charts should have only one series
        if ((config.ChartType == "Pie" || config.ChartType == "Doughnut") && config.Series.Count > 1)
        {
            result.Warnings.Add("Pie and Doughnut charts work best with a single data series");
        }

        // Stacked charts need multiple series to be meaningful
        if (config.ChartType.Contains("Stacked") && config.Series.Count == 1)
        {
            result.Warnings.Add("Stacked charts are more meaningful with multiple data series");
        }

        // 3D effects warning
        if (config.ChartStyle == "3D")
        {
            result.Warnings.Add("3D charts may be harder to read accurately. Consider using 2D for better data interpretation");
        }

        // Performance warnings for large dimensions
        if (config.Width > 1500 || config.Height > 1000)
        {
            result.Warnings.Add("Large chart dimensions may impact performance and load times");
        }

        // Animation performance warning
        if (config.IsAnimated && config.AnimationDuration > 5000)
        {
            result.Warnings.Add("Long animation durations may negatively impact user experience");
        }
    }

    private void AddPerformanceWarnings(ChartConfiguration config, ValidationResult result)
    {
        // Too many series
        if (config.Series.Count > 10)
        {
            result.Warnings.Add($"Chart has {config.Series.Count} series. Consider reducing for better readability");
        }

        // Too many filters
        if (config.Filters.Count > 5)
        {
            result.Warnings.Add($"Chart has {config.Filters.Count} filters. This may impact performance");
        }

        // Complex filter patterns
        var regexFilters = config.Filters.Count(f => f.IsRegex);
        if (regexFilters > 2)
        {
            result.Warnings.Add("Multiple regex filters may slow down data processing");
        }
    }

    private bool IsCountAggregation(string aggregationFunction)
    {
        return aggregationFunction.Equals(AggregationFunctions.Count, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsValidColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return true; // Empty is valid (uses default)

        // Check hex color format
        if (color.StartsWith("#"))
        {
            var hex = color.Substring(1);
            return hex.Length == 6 && hex.All(c => "0123456789ABCDEFabcdef".Contains(c));
        }

        // Check named colors (basic set)
        var namedColors = new[]
        {
            "red", "green", "blue", "yellow", "orange", "purple", "pink", "brown",
            "black", "white", "gray", "grey", "silver", "gold", "cyan", "magenta"
        };

        return namedColors.Contains(color.ToLowerInvariant());
    }

    private bool IsDateTimeColumn(ColumnInfo column)
    {
        return column.Type.Contains("DateTime") ||
               column.Type.Contains("Date") ||
               column.Column.ToLowerInvariant().Contains("date") ||
               column.Column.ToLowerInvariant().Contains("time");
    }
}
