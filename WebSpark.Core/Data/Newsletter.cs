namespace WebSpark.Core.Data;

public class Newsletter : BaseEntity
{
    public int PostId { get; set; }
    public bool Success { get; set; }
    public Post Post { get; set; } = null!;
}
