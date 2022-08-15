namespace ControlSpark.MineralManager.Entities;

public partial class Company
{
    public Company()
    {
        CollectionItems = new HashSet<CollectionItem>();
    }

    public int CompanyId { get; set; }
    public string CompanyNm { get; set; } = null!;
    public string? CompanyDs { get; set; }
    public int ModifiedId { get; set; }
    public DateTime ModifiedDt { get; set; }

    public virtual ICollection<CollectionItem> CollectionItems { get; set; }
}
