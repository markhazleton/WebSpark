using System.ComponentModel.DataAnnotations;

namespace WebSpark.Core.Data;

public partial class RecipeComment : BaseEntity
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Comment { get; set; }
    public virtual Data.Recipe Recipe { get; set; }
}
