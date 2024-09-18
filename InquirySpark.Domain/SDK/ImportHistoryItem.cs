namespace InquirySpark.Domain.SDK;

public class ImportHistoryItem
{
    public int ImportHistoryID { get; set; }
    public string ImportType { get; set; }
    public string FileName { get; set; }
    public string ImportLog { get; set; }
    public int NumberOfRows { get; set; }
    public int ModifiedID { get; set; }
    public DateTime ModifiedDT { get; set; }
}