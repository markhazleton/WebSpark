namespace ControlSpark.MineralManager.Entities;

public partial class CollectionItemMineral
{
    public int CollectionItemMineralId { get; set; }
    public int CollectionItemId { get; set; }
    public int MineralId { get; set; }
    public int Position { get; set; }
    public int ModifiedId { get; set; }
    public DateTime ModifiedDt { get; set; }

    public virtual CollectionItem CollectionItem { get; set; } = null!;
    public virtual Mineral Mineral { get; set; } = null!;
}
