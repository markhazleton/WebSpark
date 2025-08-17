namespace WebSpark.Core.Data;

public partial class RecipeImage : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FileDescription { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public byte[] ImageData { get; set; } = [];
    public virtual Recipe Recipe { get; set; } = null!;
}
