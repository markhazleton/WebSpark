namespace ControlSpark.MineralManager.Entities;

public partial class Mineral
{
    public Mineral()
    {
        CollectionItemMinerals = new HashSet<CollectionItemMineral>();
        CollectionItems = new HashSet<CollectionItem>();
    }

    public int MineralId { get; set; }
    public string MineralNm { get; set; } = null!;
    public string? MineralDs { get; set; }
    public string? WikipediaUrl { get; set; }
    public int ModifiedId { get; set; }
    public DateTime ModifiedDt { get; set; }

    public virtual ICollection<CollectionItemMineral> CollectionItemMinerals { get; set; }
    public virtual ICollection<CollectionItem> CollectionItems { get; set; }
}
