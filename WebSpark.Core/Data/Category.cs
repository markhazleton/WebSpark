namespace WebSpark.Core.Data;

public class Category : BaseEntity
{
    [Required]
    [StringLength(120)]
    public string Content { get; set; }
    [StringLength(255)]
    public string Description { get; set; }
    public List<PostCategory> PostCategories { get; set; }
}
