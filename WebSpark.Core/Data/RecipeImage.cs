namespace WebSpark.Core.Data;

public partial class RecipeImage : BaseEntity
{
    [Key]
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FileDescription { get; set; }
    public int DisplayOrder { get; set; }
    public byte[] ImageData { get; set; }

    public virtual Data.Recipe Recipe { get; set; }
}
