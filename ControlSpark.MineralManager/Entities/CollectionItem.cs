namespace ControlSpark.MineralManager.Entities;

public partial class CollectionItem
{
    public CollectionItem()
    {
        CollectionItemImages = new HashSet<CollectionItemImage>();
        CollectionItemMinerals = new HashSet<CollectionItemMineral>();
    }

    public int CollectionItemId { get; set; }
    public int CollectionId { get; set; }
    public double SpecimenNumber { get; set; }
    public string? Nickname { get; set; }
    public int? PrimaryMineralId { get; set; }
    public string? MineralVariety { get; set; }
    public string? MineNm { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? Value { get; set; }
    public string? ShowWherePurchased { get; set; }
    public int? PurchasedFromCompanyId { get; set; }
    public string? StorageLocation { get; set; }
    public string? SpecimenNotes { get; set; }
    public string? Description { get; set; }
    public string? ExCollection { get; set; }
    public double? HeightCm { get; set; }
    public double? WidthCm { get; set; }
    public double? ThicknessCm { get; set; }
    public double? HeightIn { get; set; }
    public double? WidthIn { get; set; }
    public double? ThicknessIn { get; set; }
    public string? WeightGr { get; set; }
    public string? WeightKg { get; set; }
    public DateTime? SaleDt { get; set; }
    public decimal? SalePrice { get; set; }
    public int? LocationCityId { get; set; }
    public int? LocationStateId { get; set; }
    public int? LocationCountryId { get; set; }
    public int ModifiedId { get; set; }
    public DateTime ModifiedDt { get; set; }
    public int IsFeatured { get; set; }
    public int IsSold { get; set; }

    public virtual Collection Collection { get; set; } = null!;
    public virtual LocationCity? LocationCity { get; set; }
    public virtual LocationCountry? LocationCountry { get; set; }
    public virtual LocationState? LocationState { get; set; }
    public virtual Mineral? PrimaryMineral { get; set; }
    public virtual Company? PurchasedFromCompany { get; set; }
    public virtual ICollection<CollectionItemImage> CollectionItemImages { get; set; }
    public virtual ICollection<CollectionItemMineral> CollectionItemMinerals { get; set; }
}
