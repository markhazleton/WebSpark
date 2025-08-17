namespace WebSpark.Core.Data;

public class PostCategory : BaseEntity
{
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
