
namespace WebSpark.Domain.Entities;

public class PostCategory : BaseEntity
{
    public int PostId { get; set; }
    public Post Post { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }
}
