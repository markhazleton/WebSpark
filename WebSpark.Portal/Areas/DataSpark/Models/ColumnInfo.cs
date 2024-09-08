namespace WebSpark.Portal.Areas.DataSpark.Models;

public class ColumnInfo
{
    public bool IsNumeric { get; set; }
    public bool IsCategory { get; set; }
    public string Column { get; set; }
    public string Type { get; set; }
    public long NonNullCount { get; set; }
    public long NullCount { get; set; }
    public long UniqueCount { get; set; }
    public object MostCommonValue { get; set; }
    public double Skewness { get; set; }
    public object Min { get; set; }
    public object Max { get; set; }
    public double Mean { get; set; }
    public double StandardDeviation { get; set; }
    public List<string> Errors { get; set; } = new List<string>(); // New property for capturing errors
    public List<string> Observations { get; set; } = new List<string>(); // New property for storing detailed observations
}

