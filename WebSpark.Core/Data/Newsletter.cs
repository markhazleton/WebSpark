namespace WebSpark.Core.Data;

public class Newsletter : BaseEntity
{
    [Key]
    public int Id { get; set; }
    public int PostId { get; set; }
    public bool Success { get; set; }
    public Post Post { get; set; }
}
