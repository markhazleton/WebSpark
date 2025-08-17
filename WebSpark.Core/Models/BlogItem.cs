namespace WebSpark.Core.Models;

public class BlogItem
{
    [Required]
    [StringLength(160)]
    public string Title { get; set; } = "Blog Title";
    [Required]
    [StringLength(255)]
    public string Description { get; set; } = string.Empty;
    [Display(Name = "Items per page")]
    public int ItemsPerPage { get; set; }
    [StringLength(160)]
    [Display(Name = "Blog cover URL")]
    public string Cover { get; set; } = string.Empty;
    [StringLength(160)]
    [Display(Name = "Blog logo URL")]
    public string Logo { get; set; } = string.Empty;
    [Required]
    [StringLength(120)]
    public string Theme { get; set; } = string.Empty;
    [Required]
    [StringLength(15)]
    public string Culture { get; set; } = string.Empty;
    public bool IncludeFeatured { get; set; }

    public List<SocialField> SocialFields { get; set; } = new();

    public string HeaderScript { get; set; } = string.Empty;
    public string FooterScript { get; set; } = string.Empty;

    [JsonIgnore]
    public dynamic values { get; set; } = default!;
}
