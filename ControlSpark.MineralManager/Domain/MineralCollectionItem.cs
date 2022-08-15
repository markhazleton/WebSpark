namespace ControlSpark.MineralManager.Domain;

public class MineralCollectionItem
{
    public int CollectionItemID { get; set; }
    public int CollectionID { get; set; }
    public string CollectionNM { get; set; }
    public double SpecimenNumber { get; set; }
    public string ImageFileNM { get; set; }
    public string Nickname { get; set; }
    public int? PrimaryMineralID { get; set; }
    public string PrimaryMineralNM { get; set; }
    public string MineralVariety { get; set; }
    public string MineNM { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? Value { get; set; }
    public string ShowWherePurchased { get; set; }
    public int? PurchasedFromCompanyID { get; set; }
    public string PurchasedFromCompanyNM { get; set; }
    public string StorageLocation { get; set; }
    public string SpecimenNotes { get; set; }
    public string Description { get; set; }
    public string ExCollection { get; set; }
    public double? HeightCm { get; set; }
    public double? WidthCm { get; set; }
    public double? ThicknessCm { get; set; }
    public double? HeightIn { get; set; }
    public double? WidthIn { get; set; }
    public double? ThicknessIn { get; set; }
    public string WeightGr { get; set; }
    public string WeightKg { get; set; }
    public DateTime? SaleDT { get; set; }
    public decimal? SalePrice { get; set; }
    public int? LocationCityID { get; set; }
    public string City { get; set; }
    public int? LocationStateID { get; set; }
    public string StateNM { get; set; }
    public int? LocationCountryID { get; set; }
    public string CountryNM { get; set; }
    public int ModifiedID { get; set; }
    public DateTime ModifiedDT { get; set; }
    public int IsFeatured { get; set; }
    public int IsSold { get; set; }
    public MineralImageList Images { get; set; } = new MineralImageList();
    public MineralItemList CollectionItemMinerals { get; set; } = new MineralItemList();
}