namespace WebSpark.Core.Data;

public class Post : BaseEntity
{
    public int AuthorId { get; set; }
    public Blog Blog { get; set; }
    [Required]
    public string Content { get; set; }
    [StringLength(160)]
    public string Cover { get; set; }
    [Required]
    [StringLength(450)]
    public string Description { get; set; }
    public bool IsFeatured { get; set; }
    public List<PostCategory> PostCategories { get; set; }

    public Models.PostType PostType { get; set; }
    public int PostViews { get; set; }
    public DateTime Published { get; set; }
    public double Rating { get; set; }
    public bool Selected { get; set; }
    [Required]
    [StringLength(160)]
    public string Slug { get; set; }
    [Required]
    [StringLength(160)]
    public string Title { get; set; }
}
