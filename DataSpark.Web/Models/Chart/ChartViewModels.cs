using DataSpark.Web.Models;
using DataSpark.Web.Models.Chart;

namespace DataSpark.Web.Models.Chart;

/// <summary>
/// View model for the main chart index page
/// </summary>
public class ChartIndexViewModel
{
    public List<ChartConfigurationSummary> SavedConfigurations { get; set; } = new();
    public List<string> AvailableDataSources { get; set; } = new();
    public string? ActiveDataSource { get; set; }
    public ChartConfiguration? CurrentConfiguration { get; set; }
    public Dictionary<string, int> ConfigurationCounts { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
}

/// <summary>
/// View model for chart configuration page
/// </summary>
public class ChartConfigurationViewModel
{
    public ChartConfiguration Configuration { get; set; } = new();
    public List<ColumnInfo> AvailableColumns { get; set; } = new();
    public List<string> ChartTypes { get; set; } = new();
    public List<string> ColorPalettes { get; set; } = new();
    public Dictionary<string, List<string>> ColumnValues { get; set; } = new();
    public List<ChartConfigurationSummary> SavedConfigurations { get; set; } = new();
    public string? DataSource { get; set; }
    public bool IsEditMode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public ProcessedChartData? PreviewData { get; set; }
}

/// <summary>
/// View model for chart display page
/// </summary>
public class ChartDisplayViewModel
{
    public ChartConfiguration Configuration { get; set; } = new();
    public string? ChartJson { get; set; }
    public string? ChartHtml { get; set; }
    public ProcessedChartData? Data { get; set; }
    public bool IsEditable { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public DataSummary? Summary { get; set; }
    public List<ExportOption> ExportOptions { get; set; } = new();
}

/// <summary>
/// Summary information for chart configurations
/// </summary>
public class ChartConfigurationSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CsvFile { get; set; } = string.Empty;
    public string ChartType { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int SeriesCount { get; set; }
    public int FilterCount { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Request model for chart data processing
/// </summary>
public class ChartDataRequest
{
    public string? XColumn { get; set; }
    public List<string> YColumns { get; set; } = new();
    public List<string> GroupByColumns { get; set; } = new();
    public List<FilterRequest> Filters { get; set; } = new();
    public string AggregationFunction { get; set; } = "Sum";
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}

/// <summary>
/// Filter request for data processing
/// </summary>
public class FilterRequest
{
    public string Column { get; set; } = string.Empty;
    public string FilterType { get; set; } = "Include";
    public List<string> Values { get; set; } = new();
    public string? MinValue { get; set; }
    public string? MaxValue { get; set; }
    public string? Pattern { get; set; }
    public bool IsCaseSensitive { get; set; }
    public bool IsRegex { get; set; }
}

/// <summary>
/// Response model for API endpoints
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? Message { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResult(params string[] errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = errors.ToList()
        };
    }

    public static ApiResponse<T> ErrorResult(string error, List<string>? warnings = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = new List<string> { error },
            Warnings = warnings ?? new List<string>()
        };
    }
}

/// <summary>
/// Export option for charts
/// </summary>
public class ExportOption
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public bool RequiresServerProcessing { get; set; }
}

/// <summary>
/// Chart configuration validation request
/// </summary>
public class ValidationRequest
{
    public ChartConfiguration Configuration { get; set; } = new();
    public string? DataSource { get; set; }
    public bool ValidateData { get; set; } = true;
}

/// <summary>
/// Chart preview request
/// </summary>
public class ChartPreviewRequest
{
    public ChartConfiguration Configuration { get; set; } = new();
    public string? DataSource { get; set; }
    public bool IncludeData { get; set; } = true;
    public int MaxDataPoints { get; set; } = 1000;
}

/// <summary>
/// Bulk operations request
/// </summary>
public class BulkOperationRequest
{
    public List<int> ConfigurationIds { get; set; } = new();
    public string Operation { get; set; } = string.Empty; // Delete, Export, Duplicate
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Chart sharing configuration
/// </summary>
public class ChartSharingConfig
{
    public int ConfigurationId { get; set; }
    public string ShareType { get; set; } = string.Empty; // Link, Embed, Export
    public bool IsPublic { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? AccessKey { get; set; }
    public List<string> AllowedDomains { get; set; } = new();
}

/// <summary>
/// Chart template for common configurations
/// </summary>
public class ChartTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ChartConfiguration Template { get; set; } = new();
    public List<string> RequiredColumns { get; set; } = new();
    public Dictionary<string, string> ColumnMappings { get; set; } = new();
}

/// <summary>
/// Available chart templates
/// </summary>
public static class ChartTemplates
{
    public static readonly List<ChartTemplate> All = new()
    {
        new ChartTemplate
        {
            Name = "Sales by Month",
            Description = "Monthly sales trend analysis",
            Category = "Time Series",
            Template = new ChartConfiguration
            {
                ChartType = "Line",
                Title = "Monthly Sales Trend",
                ShowLegend = true,
                IsAnimated = true
            },
            RequiredColumns = new List<string> { "Date", "Sales" }
        },
        new ChartTemplate
        {
            Name = "Category Comparison",
            Description = "Compare values across categories",
            Category = "Comparison",
            Template = new ChartConfiguration
            {
                ChartType = "Column",
                Title = "Category Comparison",
                ShowLegend = false,
                IsAnimated = true
            },
            RequiredColumns = new List<string> { "Category", "Value" }
        },
        new ChartTemplate
        {
            Name = "Market Share",
            Description = "Market share distribution",
            Category = "Distribution",
            Template = new ChartConfiguration
            {
                ChartType = "Pie",
                Title = "Market Share Distribution",
                ShowLegend = true,
                IsAnimated = true
            },
            RequiredColumns = new List<string> { "Category", "Share" }
        }
    };
}
