using DataSpark.Web.Models.Chart;

namespace DataSpark.Web.Models
{
    public class ChartConfigurationViewModel
    {
        public ChartConfiguration Configuration { get; set; } = new ChartConfiguration();
        public string DataSource { get; set; } = string.Empty;
        public bool IsEditMode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;
        public List<ColumnInfo> AvailableColumns { get; set; } = new List<ColumnInfo>();
        public List<string> ChartTypes { get; set; } = new List<string>();
        public List<string> ColorPalettes { get; set; } = new List<string>();
    }

    public class ChartPreviewRequest
    {
        public ChartConfiguration Configuration { get; set; } = new ChartConfiguration();
        public string DataSource { get; set; } = string.Empty;
        public bool IncludeData { get; set; } = true;
        public int MaxDataPoints { get; set; } = 1000;
    }

    public class ChartPreviewResponse
    {
        public bool Success { get; set; }
        public string ChartJson { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public ProcessedChartData? ProcessedData { get; set; }
        public string PreviewUrl { get; set; } = string.Empty;
    }

    public class ChartListViewModel
    {
        public List<ChartConfiguration> Charts { get; set; } = new List<ChartConfiguration>();
        public string DataSource { get; set; } = string.Empty;
        public List<string> AvailableDataSources { get; set; } = new List<string>();
        public string ViewMode { get; set; } = "list"; // "list" or "grid"
        public string SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
        public string SearchTerm { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class ChartViewViewModel
    {
        public ChartConfiguration Configuration { get; set; } = new ChartConfiguration();
        public ProcessedChartData ChartData { get; set; } = new ProcessedChartData();
        public string ChartJson { get; set; } = string.Empty;
        public string DataSource { get; set; } = string.Empty;
        public bool IsEmbedMode { get; set; }
        public List<string> ExportFormats { get; set; } = new List<string> { "PNG", "JPEG", "PDF", "SVG", "CSV", "Excel" };
        public Dictionary<string, object> ChartOptions { get; set; } = new Dictionary<string, object>();
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class ChartEmbedViewModel
    {
        public ChartConfiguration Configuration { get; set; } = new ChartConfiguration();
        public ProcessedChartData ChartData { get; set; } = new ProcessedChartData();
        public string ChartJson { get; set; } = string.Empty;
        public string EmbedCode { get; set; } = string.Empty;
        public string ShareUrl { get; set; } = string.Empty;
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
        public bool ShowTitle { get; set; } = true;
        public bool ShowLegend { get; set; } = true;
        public bool AllowInteraction { get; set; } = true;
        public string Theme { get; set; } = "light";
    }

    public class BulkChartOperationRequest
    {
        public List<int> ChartIds { get; set; } = new List<int>();
        public string Operation { get; set; } = string.Empty; // "delete", "duplicate", "export", "activate", "deactivate"
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    public class BulkChartOperationResponse
    {
        public bool Success { get; set; }
        public int ProcessedCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public Dictionary<int, string> Results { get; set; } = new Dictionary<int, string>();
    }

    public class ChartExportRequest
    {
        public int ChartId { get; set; }
        public string Format { get; set; } = "PNG"; // PNG, JPEG, PDF, SVG, CSV, Excel
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
        public bool IncludeData { get; set; } = false;
        public bool IncludeConfiguration { get; set; } = false;
        public string Title { get; set; } = string.Empty;
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }

    public class ChartExportResponse
    {
        public bool Success { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string DownloadUrl { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class ChartValidationRequest
    {
        public ChartConfiguration Configuration { get; set; } = new ChartConfiguration();
        public string DataSource { get; set; } = string.Empty;
        public bool ValidateData { get; set; } = true;
        public bool ValidateConfiguration { get; set; } = true;
        public bool ValidateRenderability { get; set; } = true;
    }

    public class ChartValidationResponse
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
        public List<ValidationWarning> Warnings { get; set; } = new List<ValidationWarning>();
        public Dictionary<string, object> ValidationDetails { get; set; } = new Dictionary<string, object>();
    }

    public class ValidationError
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public object? Value { get; set; }
    }

    public class ValidationWarning
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string WarningCode { get; set; } = string.Empty;
        public object? Value { get; set; }
    }
}
