
namespace WebSpark.Domain.Entities;

/// <summary>
/// Base Entity
/// </summary>
public class BaseEntity
{
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public int? UpdatedID { get; set; }
    public int? CreatedID { get; set; }
}
