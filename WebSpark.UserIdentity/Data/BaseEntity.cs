namespace WebSpark.Domain.User.Data;

public abstract class BaseEntity
{
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

