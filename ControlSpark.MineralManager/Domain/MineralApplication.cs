using ControlSpark.Domain.Models;
using ControlSpark.MineralManager.Entities;

namespace ControlSpark.MineralManager.Domain;


public class MineralApplication
{

    public MineralApplication(MineralDbContext myCon)
    {
        ShowLookup.AddRange((from i in myCon.CollectionItems
                             where !string.IsNullOrEmpty(i.ShowWherePurchased.Trim())
                             orderby i.ShowWherePurchased
                             select new LookupModel() { Value = i.ShowWherePurchased, Text = i.ShowWherePurchased }).Distinct().ToList());
        StorageLocationLookup.AddRange((from i in myCon.CollectionItems
                                        select new LookupModel() { Text = i.StorageLocation.Trim(), Value = i.StorageLocation.Trim() }).Distinct().ToList());
        MineLookup.AddRange((from i in myCon.CollectionItems
                             where !string.IsNullOrEmpty(i.MineNm.Trim())
                             orderby i.MineNm.Trim()
                             select new LookupModel() { Text = i.MineNm, Value = i.MineNm }).Distinct().ToList());
        PurchasedFromLookup.AddRange((from i in myCon.Companies
                                      orderby i.CompanyNm
                                      select new LookupModel() { Text = i.CompanyNm, Value = i.CompanyId.ToString() }).Distinct().ToList());
        MineralLookup.AddRange((from i in myCon.Minerals
                                orderby i.MineralNm
                                select new LookupModel() { Text = i.MineralNm, Value = i.MineralId.ToString() }).Distinct().ToList());
        CityLookup.AddRange((from i in myCon.LocationCities
                             orderby i.LocationCountry.CountryNm, i.City
                             select new LookupModel() { Value = i.LocationCityId.ToString(), Text = $"{i.City} - ({i.LocationState.StateNm}, {i.LocationCountry.CountryNm})" }).ToList());
        StateLookup.AddRange((from i in myCon.LocationStates
                              orderby i.LocationCountry.CountryNm, i.StateNm
                              select new LookupModel() { Value = i.LocationStateId.ToString(), Text = $"{i.StateNm} - ({i.LocationCountry.CountryNm})" }).ToList());
        CountryLookup.AddRange((from i in myCon.LocationCountries
                                orderby i.CountryNm
                                select new LookupModel() { Value = i.LocationCountryId.ToString(), Text = i.CountryNm }).Distinct().ToList());
        CollectionLookup.AddRange((from i in myCon.Collections
                                   orderby i.CollectionNm
                                   select new LookupModel() { Text = i.CollectionNm, Value = i.CollectionId.ToString() }).Distinct().ToList());
    }

    public List<LookupModel> CityLookup { get; set; } = new List<LookupModel>();
    public List<LookupModel> CollectionLookup { get; set; } = new List<LookupModel>();
    public List<LookupModel> CountryLookup { get; set; } = new List<LookupModel>();
    public List<LookupModel> MineLookup { get; set; } = new List<LookupModel>();
    public List<LookupModel> MineralLookup { get; set; } = new List<LookupModel>();
    public List<LookupModel> PurchasedFromLookup { get; set; } = new List<LookupModel>();
    public List<LookupModel> ShowLookup { get; set; } = new List<LookupModel>();
    public List<LookupModel> StateLookup { get; set; } = new List<LookupModel>();
    public List<LookupModel> StorageLocationLookup { get; set; } = new List<LookupModel>();
}