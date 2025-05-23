namespace WebSpark.Core.Data;

public class Blog : BaseEntity
{
    [StringLength(160)]
    public string Title { get; set; }
    [StringLength(450)]
    public string Description { get; set; }
    [StringLength(160)]
    public string Theme { get; set; }
    public bool IncludeFeatured { get; set; }
    public int ItemsPerPage { get; set; }
    [StringLength(160)]
    public string? Cover { get; set; }
    [StringLength(160)]
    public string? Logo { get; set; }
    [StringLength(2000)]
    public string? HeaderScript { get; set; }
    [StringLength(2000)]
    public string? FooterScript { get; set; }
    public int AnalyticsListType { get; set; }
    public int AnalyticsPeriod { get; set; }
    public List<Post> Posts { get; set; }
    public List<Author> Authors { get; set; }
}
