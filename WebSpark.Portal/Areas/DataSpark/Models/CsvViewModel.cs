using Microsoft.Data.Analysis;

namespace WebSpark.Portal.Areas.DataSpark.Models;

public class CsvViewModel
{
    public DataFrame Head { get; set; } = new();
    public List<ColumnInfo> ColumnDetails { get; set; } = [];
    public List<BivariateAnalysis> BivariateAnalyses { get; set; } = [];
    public DataFrame Description { get; set; } = new();
    public string Message { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long RowCount { get; set; }
    public int ColumnCount { get; set; }
    public DataFrame Info { get; internal set; } = new();
}
