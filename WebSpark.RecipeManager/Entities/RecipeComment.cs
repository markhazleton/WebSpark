using WebSpark.Domain.Entities;

namespace WebSpark.RecipeManager.Entities;

public partial class RecipeComment : BaseEntity
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Comment { get; set; }
    public virtual Recipe Recipe { get; set; }
}
