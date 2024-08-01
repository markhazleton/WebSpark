namespace WebSpark.Core.Data;

public partial class RecipeComment : BaseEntity
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Comment { get; set; }
    public virtual required Recipe Recipe { get; set; }
}
