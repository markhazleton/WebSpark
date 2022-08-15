namespace ControlSpark.MineralManager.Entities;

public partial class CollectionItemImage
{
    public int CollectionItemImageId { get; set; }
    public int CollectionItemId { get; set; }
    public string ImageType { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public string ImageNm { get; set; } = null!;
    public string? ImageDs { get; set; }
    public string ImageFileNm { get; set; } = null!;
    public int ModifiedId { get; set; }
    public DateTime ModifiedDt { get; set; }

    public virtual CollectionItem CollectionItem { get; set; } = null!;
}
