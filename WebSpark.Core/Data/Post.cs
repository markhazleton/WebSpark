namespace WebSpark.Core.Data;

public class Post : BaseEntity
{
    public int AuthorId { get; set; }
    public Blog Blog { get; set; } = null!;
    [Required]
    public string Content { get; set; } = string.Empty;
    [StringLength(160)]
    public string Cover { get; set; } = string.Empty;
    [Required]
    [StringLength(450)]
    public string Description { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public List<PostCategory> PostCategories { get; set; } = new();

    public Models.PostType PostType { get; set; }
    public int PostViews { get; set; }
    public DateTime Published { get; set; }
    public double Rating { get; set; }
    public bool Selected { get; set; }
    [Required]
    [StringLength(160)]
    public string Slug { get; set; } = string.Empty;
    [Required]
    [StringLength(160)]
    public string Title { get; set; } = string.Empty;
}
