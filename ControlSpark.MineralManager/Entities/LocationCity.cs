namespace ControlSpark.MineralManager.Entities;

public partial class LocationCity
{
    public LocationCity()
    {
        CollectionItems = new HashSet<CollectionItem>();
    }

    public int LocationCityId { get; set; }
    public string City { get; set; } = null!;
    public string? CityDs { get; set; }
    public string? County { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public int? LocationStateId { get; set; }
    public int? LocationCountryId { get; set; }
    public int ModifiedId { get; set; }
    public DateTime ModifiedDt { get; set; }

    public virtual LocationCountry? LocationCountry { get; set; }
    public virtual LocationState? LocationState { get; set; }
    public virtual ICollection<CollectionItem> CollectionItems { get; set; }
}
