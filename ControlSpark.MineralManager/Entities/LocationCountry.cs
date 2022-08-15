namespace ControlSpark.MineralManager.Entities;

public partial class LocationCountry
{
    public LocationCountry()
    {
        CollectionItems = new HashSet<CollectionItem>();
        LocationCities = new HashSet<LocationCity>();
        LocationStates = new HashSet<LocationState>();
    }

    public int LocationCountryId { get; set; }
    public string CountryNm { get; set; } = null!;
    public string? CountryDs { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public int ModifiedId { get; set; }
    public DateTime ModifiedDt { get; set; }

    public virtual ICollection<CollectionItem> CollectionItems { get; set; }
    public virtual ICollection<LocationCity> LocationCities { get; set; }
    public virtual ICollection<LocationState> LocationStates { get; set; }
}
