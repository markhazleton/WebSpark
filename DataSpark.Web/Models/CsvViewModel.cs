using Microsoft.Data.Analysis;

namespace DataSpark.Web.Models;

public class CsvViewModel
{
    public List<string> AvailableCsvFiles { get; set; } = [];
    public List<BivariateAnalysis> BivariateAnalyses { get; set; } = [];
    public int ColumnCount { get; set; }
    public List<ColumnInfo> ColumnDetails { get; set; } = [];
    public DataFrame Description { get; set; } = new();
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DataFrame Head { get; set; } = new();
    public DataFrame Info { get; internal set; } = new();
    public string Message { get; set; } = string.Empty;
    public long RowCount { get; set; }
}

