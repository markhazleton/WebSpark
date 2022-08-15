﻿namespace ControlSpark.MineralManager.Entities;

public partial class VwCollectionItemImage
{
    public int CollectionItemId { get; set; }
    public double SpecimenNumber { get; set; }
    public string? Nickname { get; set; }
    public string? CollectionNm { get; set; }
    public int? PrimaryMineralId { get; set; }
    public string? MineralNm { get; set; }
    public string? MineralDs { get; set; }
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
    public string? CompanyNm { get; set; }
    public string? City { get; set; }
    public string? CompanyDs { get; set; }
    public string? CityDs { get; set; }
    public string? CountryNm { get; set; }
    public string? CountryDs { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? StateNm { get; set; }
    public string? StateCd { get; set; }
    public string? StateDs { get; set; }
    public int CollectionId { get; set; }
    public int? CollectionItemImageId { get; set; }
    public string? ImageType { get; set; }
    public int? DisplayOrder { get; set; }
    public string? ImageNm { get; set; }
    public string? ImageDs { get; set; }
    public string? ImageFileNm { get; set; }
    public int? ModifiedId { get; set; }
    public DateTime? ModifiedDt { get; set; }
    public int IsFeatured { get; set; }
    public int IsSold { get; set; }
}
