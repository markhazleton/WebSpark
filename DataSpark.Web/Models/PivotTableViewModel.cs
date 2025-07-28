namespace DataSpark.Web.Models
{
    public class PivotTableViewModel
    {
        public string CurrentFile { get; set; } = string.Empty;
        public List<string> AvailableFiles { get; set; } = new List<string>();
        public List<string> ColumnHeaders { get; set; } = new List<string>();
        public int RecordCount { get; set; }
        public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();
    }

    public class LoadCsvDataRequest
    {
        public string FileName { get; set; } = string.Empty;
        public List<string>? SelectedColumns { get; set; }
    }

    public class LoadCsvDataResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();
        public List<string> Columns { get; set; } = new List<string>();
        public int RecordCount { get; set; }
    }

    public class SaveConfigurationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CsvFile { get; set; } = string.Empty;
        public string AggregatorName { get; set; } = string.Empty;
        public string RendererName { get; set; } = string.Empty;
        public List<string> Cols { get; set; } = new List<string>();
        public List<string> Rows { get; set; } = new List<string>();
        public List<string> Vals { get; set; } = new List<string>();
        public Dictionary<string, object> IncludeValues { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> ExcludeValues { get; set; } = new Dictionary<string, object>();
    }

    public class PivotTableConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CsvFile { get; set; } = string.Empty;
        public string AggregatorName { get; set; } = string.Empty;
        public string RendererName { get; set; } = string.Empty;
        public List<string> Cols { get; set; } = new List<string>();
        public List<string> Rows { get; set; } = new List<string>();
        public List<string> Vals { get; set; } = new List<string>();
        public Dictionary<string, object> IncludeValues { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> ExcludeValues { get; set; } = new Dictionary<string, object>();
        public DateTime CreatedAt { get; set; }
    }

    public class StandardResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
