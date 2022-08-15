namespace ControlSpark.MineralManager.Entities;

public partial class LocationState
{
    public LocationState()
    {
        CollectionItems = new HashSet<CollectionItem>();
        LocationCities = new HashSet<LocationCity>();
    }

    public int LocationStateId { get; set; }
    public string StateNm { get; set; } = null!;
    public string? StateCd { get; set; }
    public string? StateDs { get; set; }
    public int? LocationCountryId { get; set; }
    public int ModifiedId { get; set; }
    public DateTime ModifiedDt { get; set; }

    public virtual LocationCountry? LocationCountry { get; set; }
    public virtual ICollection<CollectionItem> CollectionItems { get; set; }
    public virtual ICollection<LocationCity> LocationCities { get; set; }
}
