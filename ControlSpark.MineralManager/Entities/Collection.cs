namespace ControlSpark.MineralManager.Entities;

public partial class Collection
{
    public Collection()
    {
        CollectionItems = new HashSet<CollectionItem>();
    }

    public int CollectionId { get; set; }
    public string CollectionNm { get; set; } = null!;
    public string? CollectionDs { get; set; }
    public int ModifiedId { get; set; }
    public DateTime ModifiedDt { get; set; }

    public virtual ICollection<CollectionItem> CollectionItems { get; set; }
}
