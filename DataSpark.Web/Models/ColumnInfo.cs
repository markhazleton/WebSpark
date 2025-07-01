namespace DataSpark.Web.Models;

public class ColumnInfo
{
    public bool IsNumeric { get; set; }
    public bool IsCategory { get; set; }
    public string Column { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public long NonNullCount { get; set; }
    public long NullCount { get; set; }
    public long UniqueCount { get; set; }
    public object? MostCommonValue { get; set; }
    public double Skewness { get; set; }
    public object? Min { get; set; }
    public object? Max { get; set; }
    public double Mean { get; set; }
    public double StandardDeviation { get; set; }
    public double Median { get; set; }
    public double Q1 { get; set; }
    public double Q3 { get; set; }
    public double IQR { get; set; }
    public List<string> Errors { get; set; } = []; // New property for capturing errors
    public List<string> Observations { get; set; } = []; // New property for storing detailed observations
}

