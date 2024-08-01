namespace WebSpark.Core.Data;

/// <summary>
/// Base Entity
/// </summary>
public class BaseEntity
{
    [Key]
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public int? UpdatedID { get; set; }
    public int? CreatedID { get; set; }
}
