namespace WebSpark.SharedKernel;

/// <summary>
/// Base type for all entities which track state using a given Id.
/// </summary>
/// <typeparam name="TId">Type of the entity identifier.</typeparam>
public abstract class BaseEntity<TId>
{
    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    public TId? Id { get; protected set; }

    /// <summary>
    /// Gets the list of domain events associated with this entity.
    /// </summary>
    public List<BaseDomainEvent> Events { get; } = new();

    /// <summary>
    /// Gets or sets the date the entity was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the date the entity was last updated.
    /// </summary>
    public DateTime UpdatedDate { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated the entity.
    /// </summary>
    public int? UpdatedID { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    public int? CreatedID { get; set; }
}
