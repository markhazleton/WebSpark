namespace InquirySpark.Domain.SDK;

public class SiteMessageItem
{
    public int Id { get; set; }
    public int? ToUserID { get; set; }
    public int? FromUserID { get; set; }
    public string Message { get; set; }
    public bool? Opened { get; set; }
    public DateTime? CratedDateTime { get; set; }
    public string Subject { get; set; }
    public bool? Deleted { get; set; }
    public int? AppID { get; set; }
    public int? ShowonPage { get; set; }
    public bool? FromApp { get; set; }
}