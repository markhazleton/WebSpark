namespace TriviaSpark.Domain.Entities;

public abstract class BaseEntity
{
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

