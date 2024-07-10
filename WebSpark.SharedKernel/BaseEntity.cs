namespace WebSpark.SharedKernel;

/// <summary>
/// Base types for all Entities which track state using a given Id.
/// </summary>
public abstract class BaseEntity<TId>
{
    public TId Id { get; protected set; }
    public List<BaseDomainEvent> Events = [];
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public int? UpdatedID { get; set; }
    public int? CreatedID { get; set; }
}
