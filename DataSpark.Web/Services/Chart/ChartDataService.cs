using DataSpark.Web.Models;
using DataSpark.Web.Models.Chart;
using DataSpark.Web.Services.Chart;
using DataSpark.Web.Services;
using CsvHelper;
using System.Globalization;
using System.Text;

namespace DataSpark.Web.Services.Chart;

/// <summary>
/// Implementation of data processing service for charts
/// </summary>
public class ChartDataService : IDataService
{
    private readonly CsvFileService _csvFileService;
    private readonly ILogger<ChartDataService> _logger;

    public ChartDataService(CsvFileService csvFileService, ILogger<ChartDataService> logger)
    {
        _csvFileService = csvFileService;
        _logger = logger;
    }

    public async Task<ProcessedChartData> ProcessDataAsync(string dataSource, ChartConfiguration config)
    {
        try
        {
            _logger.LogInformation("Processing chart data for source: {DataSource}, Chart: {ChartName}, Type: {ChartType}",
                dataSource, config.Name, config.ChartType);

            var result = new ProcessedChartData
            {
                ProcessedDate = DateTime.UtcNow,
                ProcessingNotes = $"Processed for chart type: {config.ChartType}"
            };

            // Load the CSV data
            _logger.LogDebug("Loading CSV data from: {DataSource}", dataSource);
            var csvResult = await _csvFileService.ReadCsvForVisualizationAsync(dataSource);
            if (csvResult == null || csvResult.Records.Count == 0)
            {
                var warningMessage = $"No data found in '{dataSource}'. Please ensure the file contains data and try again.";
                result.Warnings.Add(warningMessage);
                _logger.LogWarning("No data found in data source: {DataSource}", dataSource);
                return result;
            }

            result.TotalRows = csvResult.Records.Count;
            _logger.LogDebug("Loaded {RowCount} rows from data source: {DataSource}", result.TotalRows, dataSource);

            // Get column information
            var columns = await GetColumnsAsync(dataSource);
            _logger.LogDebug("Identified {ColumnCount} columns: {Columns}",
                columns.Count, string.Join(", ", columns.Select(c => $"{c.Column}({(c.IsNumeric ? "numeric" : "text")})")));

            foreach (var column in columns)
            {
                result.ColumnTypes[column.Column] = DetermineDataType(column);
            }

            // Convert dynamic records to dictionary format for processing
            var dataDict = ConvertDynamicToDict(csvResult.Records);

            // Apply filters if any
            var filteredData = ApplyFilters(dataDict, config.Filters);
            if (filteredData.Count != csvResult.Records.Count)
            {
                result.ProcessingNotes += $". Filtered from {csvResult.Records.Count} to {filteredData.Count} rows";
                _logger.LogInformation("Applied {FilterCount} filters, data reduced from {OriginalCount} to {FilteredCount} rows",
                    config.Filters.Count, csvResult.Records.Count, filteredData.Count);
            }

            // Process each series
            var processedSeriesCount = 0;
            foreach (var series in config.Series.Where(s => s.IsVisible).OrderBy(s => s.DisplayOrder))
            {
                try
                {
                    _logger.LogDebug("Processing series: {SeriesName} -> Column: {DataColumn}, Aggregation: {Aggregation}",
                        series.Name, series.DataColumn, series.AggregationFunction);

                    var seriesData = ProcessSeries(filteredData, series, config, columns);
                    result.DataPoints.AddRange(seriesData);

                    if (!result.SeriesNames.Contains(series.Name))
                        result.SeriesNames.Add(series.Name);

                    processedSeriesCount++;
                    _logger.LogDebug("Successfully processed series: {SeriesName}, generated {DataPointCount} data points",
                        series.Name, seriesData.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing series: {SeriesName} -> {DataColumn}. Error: {Message}",
                        series.Name, series.DataColumn, ex.Message);
                    result.Warnings.Add($"Could not process series '{series.Name}' with column '{series.DataColumn}': {ex.Message}");
                }
            }

            _logger.LogInformation("Chart data processing completed for '{ChartName}': {SeriesCount} series processed, {DataPointCount} total data points",
                config.Name, processedSeriesCount, result.DataPoints.Count);

            // Extract categories
            if (config.XAxis != null && !string.IsNullOrWhiteSpace(config.XAxis.DataColumn))
            {
                result.Categories = ExtractCategories(filteredData, config.XAxis.DataColumn, config.XAxis.SelectedValues);
            }

            _logger.LogInformation("Successfully processed {DataPoints} data points for {Series} series",
                result.DataPoints.Count, result.SeriesNames.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chart data for source: {DataSource}", dataSource);
            throw;
        }
    }

    public async Task<List<ColumnInfo>> GetColumnsAsync(string dataSource)
    {
        try
        {
            _logger.LogDebug("Getting columns with type detection for data source: {DataSource}", dataSource);

            // First, get the DataFrame to detect proper types
            var dataFrameResult = await _csvFileService.ReadCsvAsDataFrameAsync(dataSource);
            if (!dataFrameResult.Success || !dataFrameResult.Data.Any())
            {
                _logger.LogWarning("Could not load DataFrame for type detection, falling back to basic headers");
                var headersResult = await _csvFileService.GetCsvHeadersAsync(dataSource);
                if (!headersResult.Success || headersResult.Data == null)
                {
                    return new List<ColumnInfo>();
                }

                return headersResult.Data.Select(header => new ColumnInfo
                {
                    Column = header,
                    Type = "string",
                    IsNumeric = false
                }).ToList();
            }

            var dataFrame = dataFrameResult.Data[0];
            var columns = new List<ColumnInfo>();

            foreach (var column in dataFrame.Columns)
            {
                var columnInfo = new ColumnInfo
                {
                    Column = column.Name
                };

                // Determine type based on DataFrame column type
                var columnType = column.DataType;

                if (columnType == typeof(int) || columnType == typeof(long) ||
                    columnType == typeof(short) || columnType == typeof(byte))
                {
                    columnInfo.Type = "Integer";
                    columnInfo.IsNumeric = true;
                }
                else if (columnType == typeof(float) || columnType == typeof(double) ||
                         columnType == typeof(decimal))
                {
                    columnInfo.Type = "Decimal";
                    columnInfo.IsNumeric = true;
                }
                else if (columnType == typeof(DateTime))
                {
                    columnInfo.Type = "DateTime";
                    columnInfo.IsNumeric = false;
                }
                else if (columnType == typeof(bool))
                {
                    columnInfo.Type = "Boolean";
                    columnInfo.IsNumeric = false;
                }
                else
                {
                    columnInfo.Type = "String";
                    columnInfo.IsNumeric = false;

                    // For string columns, try to detect if they contain numeric data
                    // by sampling some values
                    try
                    {
                        var sampleSize = Math.Min(100, (int)column.Length);
                        var numericCount = 0;
                        var totalNonNullCount = 0;

                        for (int i = 0; i < sampleSize; i++)
                        {
                            var value = column[i];
                            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                            {
                                totalNonNullCount++;
                                if (double.TryParse(value.ToString(), out _))
                                {
                                    numericCount++;
                                }
                            }
                        }

                        // If more than 80% of non-null values are numeric, treat as numeric
                        if (totalNonNullCount > 0 && (double)numericCount / totalNonNullCount > 0.8)
                        {
                            columnInfo.IsNumeric = true;
                            columnInfo.Type = "Decimal"; // Default to decimal for parsed numbers
                            _logger.LogDebug("Column {ColumnName} detected as numeric based on content analysis ({NumericCount}/{TotalCount})",
                                column.Name, numericCount, totalNonNullCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error during numeric detection for column {ColumnName}, treating as string", column.Name);
                    }
                }

                columns.Add(columnInfo);
                _logger.LogDebug("Column {ColumnName}: Type={Type}, IsNumeric={IsNumeric}",
                    columnInfo.Column, columnInfo.Type, columnInfo.IsNumeric);
            }

            _logger.LogInformation("Detected {ColumnCount} columns with types: {ColumnSummary}",
                columns.Count,
                string.Join(", ", columns.Select(c => $"{c.Column}({c.Type})")));

            return columns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting columns for data source: {DataSource}", dataSource);
            return new List<ColumnInfo>();
        }
    }

    public async Task<List<string>> GetColumnValuesAsync(string dataSource, string column, int maxValues = 1000)
    {
        try
        {
            var csvResult = await _csvFileService.ReadCsvForVisualizationAsync(dataSource);
            if (csvResult == null || csvResult.Records.Count == 0)
                return new List<string>();

            // Convert to dictionary format for easier access
            var dataDict = ConvertDynamicToDict(csvResult.Records);

            var values = dataDict
                .Select(row => row.TryGetValue(column, out var value) ? value?.ToString() ?? "" : "")
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .OrderBy(v => v)
                .Take(maxValues)
                .ToList();

            return values;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting column values for {Column} in {DataSource}", column, dataSource);
            return new List<string>();
        }
    }

    public async Task<bool> ValidateDataSourceAsync(string dataSource)
    {
        try
        {
            var columns = await GetColumnsAsync(dataSource);
            return columns.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating data source: {DataSource}", dataSource);
            return false;
        }
    }

    public async Task<List<string>> GetAvailableDataSourcesAsync()
    {
        try
        {
            // Use the synchronous method and wrap in Task.FromResult to maintain async signature
            return await Task.FromResult(_csvFileService.GetCsvFileNames());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available data sources");
            return new List<string>();
        }
    }

    public async Task<Dictionary<string, List<string>>> GetMultipleColumnValuesAsync(string dataSource, List<string> columns, int maxValues = 100)
    {
        var result = new Dictionary<string, List<string>>();

        try
        {
            foreach (var column in columns)
            {
                var values = await GetColumnValuesAsync(dataSource, column, maxValues);
                result[column] = values;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple column values for {DataSource}", dataSource);
        }

        return result;
    }

    /// <summary>
    /// Convert dynamic records to dictionary format for processing
    /// </summary>
    private List<Dictionary<string, object?>> ConvertDynamicToDict(List<dynamic> dynamicRecords)
    {
        var result = new List<Dictionary<string, object?>>();

        foreach (var record in dynamicRecords)
        {
            var dict = new Dictionary<string, object?>();

            // Convert dynamic object to dictionary
            if (record is IDictionary<string, object> expandoDict)
            {
                foreach (var kvp in expandoDict)
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }
            else
            {
                // Handle other dynamic types
                var properties = record.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    dict[prop.Name] = prop.GetValue(record);
                }
            }

            result.Add(dict);
        }

        return result;
    }

    public async Task<DataSummary> GetDataSummaryAsync(string dataSource)
    {
        try
        {
            var csvResult = await _csvFileService.ReadCsvForVisualizationAsync(dataSource);
            var columns = await GetColumnsAsync(dataSource);

            return new DataSummary
            {
                TotalDataPoints = csvResult?.Records?.Count ?? 0,
                CategoryCount = columns.Count,
                SeriesCount = columns.Count(c => c.IsNumeric),
                MinValue = 0,
                MaxValue = 0,
                AverageValue = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data summary for {DataSource}", dataSource);
            return new DataSummary();
        }
    }

    private List<Dictionary<string, object?>> ApplyFilters(List<Dictionary<string, object?>> data, List<ChartFilter> filters)
    {
        if (filters == null || filters.Count == 0 || !filters.Any(f => f.IsEnabled))
            return data;

        var filteredData = new List<Dictionary<string, object?>>();

        foreach (var row in data)
        {
            var passesAllFilters = true;

            foreach (var filter in filters.Where(f => f.IsEnabled))
            {
                var columnValue = row.TryGetValue(filter.Column, out var value) ? value?.ToString() : null;
                if (!filter.PassesFilter(columnValue))
                {
                    passesAllFilters = false;
                    break;
                }
            }

            if (passesAllFilters)
                filteredData.Add(row);
        }

        return filteredData;
    }

    private List<ChartDataPoint> ProcessSeries(List<Dictionary<string, object?>> data, ChartSeries series, ChartConfiguration config, List<ColumnInfo> columns)
    {
        var dataPoints = new List<ChartDataPoint>();

        if (string.IsNullOrWhiteSpace(series.DataColumn))
            return dataPoints;

        var dataColumn = columns.FirstOrDefault(c => c.Column == series.DataColumn);
        if (dataColumn == null)
            return dataPoints;

        // Group data by X-axis category if specified
        if (config.XAxis != null && !string.IsNullOrWhiteSpace(config.XAxis.DataColumn))
        {
            var groups = data.GroupBy(row =>
                row.TryGetValue(config.XAxis.DataColumn, out var value) ? value?.ToString() ?? "" : "");

            foreach (var group in groups)
            {
                var category = group.Key;
                var values = ExtractNumericValues(group, series.DataColumn);

                if (values.Any())
                {
                    var aggregatedValue = ApplyAggregation(values, series.AggregationFunction);

                    dataPoints.Add(new ChartDataPoint
                    {
                        Label = category,
                        Category = category,
                        Value = aggregatedValue,
                        SeriesName = series.Name,
                        Color = series.Color,
                        Tooltip = $"{series.Name}: {aggregatedValue:N2}"
                    });
                }
            }
        }
        else
        {
            // No grouping, process all values as a single series
            var values = ExtractNumericValues(data, series.DataColumn);
            if (values.Any())
            {
                var aggregatedValue = ApplyAggregation(values, series.AggregationFunction);

                dataPoints.Add(new ChartDataPoint
                {
                    Label = series.Name,
                    Category = "Total",
                    Value = aggregatedValue,
                    SeriesName = series.Name,
                    Color = series.Color,
                    Tooltip = $"{series.Name}: {aggregatedValue:N2}"
                });
            }
        }

        return dataPoints;
    }

    private List<double> ExtractNumericValues(IEnumerable<Dictionary<string, object?>> data, string column)
    {
        var values = new List<double>();

        foreach (var row in data)
        {
            if (row.TryGetValue(column, out var value) && value != null)
            {
                if (double.TryParse(value.ToString(), out var numValue))
                {
                    values.Add(numValue);
                }
            }
        }

        return values;
    }

    private double ApplyAggregation(List<double> values, string aggregationFunction)
    {
        if (!values.Any())
            return 0;

        return aggregationFunction switch
        {
            AggregationFunctions.Sum => values.Sum(),
            AggregationFunctions.Average => values.Average(),
            AggregationFunctions.Count => values.Count,
            AggregationFunctions.Min => values.Min(),
            AggregationFunctions.Max => values.Max(),
            AggregationFunctions.Median => CalculateMedian(values),
            AggregationFunctions.StandardDeviation => CalculateStandardDeviation(values),
            _ => values.Sum()
        };
    }

    private double CalculateMedian(List<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var count = sorted.Count;

        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    private double CalculateStandardDeviation(List<double> values)
    {
        if (values.Count <= 1)
            return 0;

        var mean = values.Average();
        var squaredDifferences = values.Select(v => Math.Pow(v - mean, 2));
        var variance = squaredDifferences.Average();

        return Math.Sqrt(variance);
    }

    private List<string> ExtractCategories(List<Dictionary<string, object?>> data, string column, List<string> selectedValues)
    {
        var categories = data
            .Select(row => row.TryGetValue(column, out var value) ? value?.ToString() ?? "" : "")
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct()
            .OrderBy(v => v)
            .ToList();

        // Filter by selected values if specified
        if (selectedValues != null && selectedValues.Any())
        {
            categories = categories.Where(c => selectedValues.Contains(c)).ToList();
        }

        return categories;
    }

    private DataType DetermineDataType(ColumnInfo column)
    {
        if (column.IsNumeric)
        {
            return column.Type.Contains("Int") ? DataType.Integer : DataType.Decimal;
        }

        if (column.Type.Contains("DateTime") || column.Type.Contains("Date"))
        {
            return DataType.DateTime;
        }

        if (column.Type.Contains("Boolean"))
        {
            return DataType.Boolean;
        }

        if (column.IsCategory)
        {
            return DataType.Category;
        }

        return DataType.String;
    }
}
