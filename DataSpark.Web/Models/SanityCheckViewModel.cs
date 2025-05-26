namespace DataSpark.Web.Models;

public class SanityCheckViewModel
{
    public string? FileName { get; set; }
    public List<string> AvailableFiles { get; set; } = new();
    public List<string> Headers { get; set; } = new();
    public int RowCount { get; set; }
    public List<dynamic> SampleRows { get; set; } = new();
    public bool HasMissingValues { get; set; }
}
