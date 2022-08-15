using ControlSpark.Domain.Extensions;
using ControlSpark.Domain.Models.SQLHelper;
using ControlSpark.MineralManager.Entities;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Linq;

namespace ControlSpark.MineralManager.Domain;

public class MineralCollectionListView
{
    private const int INT_IndexBase = 0;
    public int CurrentIndex { get; set; }
    public int CurrentCollectionItemID { get; set; } = 0;
    public MineralCollectionItem CurrentCollectionItem { get; set; } = new MineralCollectionItem();
    public List<MineralCollectionItem> myResults { get; set; } = new List<MineralCollectionItem>();
    public int MaxIndex
    {
        get
        {
            return myResults.Count - 1;
        }
    }
    public SQLFilterList MySQLFilter { get; set; } = new SQLFilterList();
    public bool FeaturedOnly
    {
        set
        {
            if (value)
            {
                MySQLFilter.Add(new SQLFilterClause("IsFeatured", SQLFilterOperator.Equal, "-1", SQLFilterConjunction.andConjunction, "CollectionItem"));
            }
        }
    }
    public bool SoldOnly
    {
        set
        {
            if (value)
            {
                MySQLFilter.Add(new SQLFilterClause("IsSold", SQLFilterOperator.Equal, "-1", SQLFilterConjunction.andConjunction, "CollectionItem"));
            }
            else
            {
                MySQLFilter.Add(new SQLFilterClause("IsSold", SQLFilterOperator.Equal, "0", SQLFilterConjunction.andConjunction, "CollectionItem"));

            }
        }
    }
    public string CompanyID
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                MySQLFilter.Add(new SQLFilterClause("PurchasedFromCompanyID", SQLFilterOperator.Equal, value, SQLFilterConjunction.andConjunction, "CollectionItem"));
            }
        }
    }
    public string Description
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                MySQLFilter.Add(new SQLFilterClause("Description", SQLFilterOperator.Equal, value, SQLFilterConjunction.andConjunction, "CollectionItem"));
            }
        }
    }
    public string MineNM
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                MySQLFilter.Add(new SQLFilterClause("MineNM", SQLFilterOperator.Equal, value, SQLFilterConjunction.andConjunction, "CollectionItem"));
            }
        }
    }
    public string SpecimenNumber
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                MySQLFilter.Add(new SQLFilterClause("SpecimenNumber", SQLFilterOperator.Equal, value, SQLFilterConjunction.andConjunction, "CollectionItem"));
            }
        }
    }
    public string MineralID
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (Information.IsNumeric(value))
                {
                    if (Conversions.ToInteger(value) > 0)
                    {
                        MySQLFilter.Add(new SQLFilterClause("MineralID", SQLFilterOperator.Equal, value, SQLFilterConjunction.andConjunction, "CollectionItem"));
                    }
                }
            }
        }
    }
    public string LocationCityID
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (Information.IsNumeric(value))
                {
                    if (Conversions.ToInteger(value) > 0)
                    {
                        MySQLFilter.Add(new SQLFilterClause("LocationCityID", SQLFilterOperator.Equal, value, SQLFilterConjunction.andConjunction, "CollectionItem"));
                    }
                }
            }
        }
    }
    public string LocationStateID
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (Information.IsNumeric(value))
                {
                    if (Conversions.ToInteger(value) > 0)
                    {
                        MySQLFilter.Add(new SQLFilterClause("LocationStateID", SQLFilterOperator.Equal, value, SQLFilterConjunction.andConjunction, "CollectionItem"));
                    }
                }
            }

        }
    }
    public string LocationCountryID
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (Information.IsNumeric(value))
                {
                    if (Conversions.ToInteger(value) > 0)
                    {
                        MySQLFilter.Add(new SQLFilterClause("LocationCountryID", SQLFilterOperator.Equal, value, SQLFilterConjunction.andConjunction, "CollectionItem"));
                    }
                }
            }
        }
    }
    public string CollectionItemID
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (Information.IsNumeric(value))
                {
                    CurrentCollectionItemID = Utility.wpm_GetDBInteger(value, 0);
                }
            }
        }
    }
    public string CollectionID
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                MySQLFilter.Add(new SQLFilterClause("CollectionID", SQLFilterOperator.Equal, value, SQLFilterConjunction.andConjunction, "CollectionItem"));
            }
        }
    }

    public string ResultText { get; set; }

    public void ResetCriteria()
    {
        MySQLFilter.Clear();
        CurrentIndex = 0;
    }

    public MineralCollectionItem GetCurrentItem()
    {

        if (CurrentCollectionItemID > 0)
        {
            try
            {
                using (var mycon = new MineralDbContext())
                {
                    CurrentCollectionItem = (from i in mycon.VwCollectionItems
                                             where i.CollectionItemId == CurrentCollectionItemID
                                             select new MineralCollectionItem()
                                             {
                                                 CollectionItemID = i.CollectionItemId,
                                                 CollectionID = i.CollectionId,
                                                 CollectionNM = i.CollectionNm,
                                                 SpecimenNumber = i.SpecimenNumber,
                                                 ImageFileNM = i.ImageFileNm,
                                                 Nickname = i.Nickname,
                                                 PrimaryMineralID = i.PrimaryMineralId,
                                                 PrimaryMineralNM = i.PrimaryMineralNm,
                                                 MineralVariety = i.MineralVariety,
                                                 MineNM = i.MineNm,
                                                 PurchaseDate = i.PurchaseDate,
                                                 PurchasePrice = i.PurchasePrice,
                                                 Value = i.Value,
                                                 ShowWherePurchased = i.ShowWherePurchased,
                                                 PurchasedFromCompanyID = i.PurchasedFromCompanyId,
                                                 PurchasedFromCompanyNM = i.CompanyNm,
                                                 StorageLocation = i.StorageLocation,
                                                 SpecimenNotes = i.SpecimenNotes,
                                                 Description = i.Description,
                                                 ExCollection = i.ExCollection,
                                                 HeightCm = i.HeightCm,
                                                 WidthCm = i.WidthCm,
                                                 ThicknessCm = i.ThicknessCm,
                                                 HeightIn = i.HeightIn,
                                                 WidthIn = i.WidthIn,
                                                 ThicknessIn = i.ThicknessIn,
                                                 WeightGr = i.WeightGr,
                                                 WeightKg = i.WeightKg,
                                                 SaleDT = i.SaleDt,
                                                 SalePrice = i.SalePrice,
                                                 LocationCityID = i.LocationCityId,
                                                 City = i.City,
                                                 LocationStateID = i.LocationStateId,
                                                 StateNM = i.StateNm,
                                                 LocationCountryID = i.LocationCountryId,
                                                 CountryNM = i.CountryNm,
                                                 IsSold = i.IsSold,
                                                 IsFeatured = i.IsFeatured
                                             }).SingleOrDefault();

                    int iOrder = 0;
                    var myImageList = new List<MineralImage>();
                    myImageList.AddRange((from i in mycon.CollectionItemImages
                                          where i.CollectionItemId == CurrentCollectionItemID
                                          orderby i.DisplayOrder
                                          select new MineralImage()
                                          {
                                              CollectionItemID = i.CollectionItemId,
                                              CollectionItemImageID = i.CollectionItemImageId,
                                              DisplayOrder = i.DisplayOrder,
                                              ImageDS = i.ImageDs,
                                              ImageFileNM = i.ImageFileNm,
                                              ImageNM = i.ImageNm,
                                              ImageType = i.ImageType,
                                              ModifiedDT = i.ModifiedDt,
                                              ModifiedID = i.ModifiedId
                                          }).ToList());
                    foreach (MineralImage myImage in myImageList)
                    {
                        if (myImage.ImageType == "Photo")
                        {
                            myImage.DisplayOrder = iOrder;
                            iOrder = iOrder + 1;

                            CurrentCollectionItem.Images.Add(myImage);
                        }
                    }
                    foreach (MineralImage myImage in myImageList)
                    {
                        if (myImage.ImageType != "Photo")
                        {
                            myImage.DisplayOrder = iOrder;
                            iOrder = iOrder + 1;
                            CurrentCollectionItem.Images.Add(myImage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //   ApplicationLogging.ErrorLog("MineralCollectionView.GetCurrentItem", ex.ToString());
            }
        }
        CurrentCollectionItemID = -1;
        return CurrentCollectionItem;
    }
    public List<MineralCollectionItem> GetList()
    {
        myResults.Clear();
        using (var mycon = new MineralDbContext())
        {
            try
            {
                if (MySQLFilter.FindField("MineralID").Count > 0)
                {
                    //myResults.AddRange((from i in mycon.VwMineralCollectionItems.Where(MySQLFilter.GetLINQWhere()).OrderBy("SpecimenNumber")
                    //                    select new MineralCollectionItem()
                    //                    {
                    //                        CollectionItemID = i.CollectionItemID,
                    //                        CollectionID = i.CollectionID,
                    //                        CollectionNM = i.CollectionNM,
                    //                        SpecimenNumber = i.SpecimenNumber,
                    //                        ImageFileNM = i.ImageFileNM,
                    //                        Nickname = i.Nickname,
                    //                        PrimaryMineralID = i.PrimaryMineralID,
                    //                        PrimaryMineralNM = i.PrimaryMineralNM,
                    //                        MineralVariety = i.MineralVariety,
                    //                        MineNM = i.MineNM,
                    //                        PurchaseDate = i.PurchaseDate,
                    //                        PurchasePrice = i.PurchasePrice,
                    //                        Value = i.Value,
                    //                        ShowWherePurchased = i.ShowWherePurchased,
                    //                        PurchasedFromCompanyID = i.PurchasedFromCompanyID,
                    //                        PurchasedFromCompanyNM = i.CompanyNM,
                    //                        StorageLocation = i.StorageLocation,
                    //                        SpecimenNotes = i.SpecimenNotes,
                    //                        Description = i.Description,
                    //                        ExCollection = i.ExCollection,
                    //                        HeightCm = i.HeightCm,
                    //                        WidthCm = i.WidthCm,
                    //                        ThicknessCm = i.ThicknessCm,
                    //                        HeightIn = i.HeightIn,
                    //                        WidthIn = i.WidthIn,
                    //                        ThicknessIn = i.ThicknessIn,
                    //                        WeightGr = i.WeightGr,
                    //                        WeightKg = i.WeightKg,
                    //                        SaleDT = i.SaleDT,
                    //                        SalePrice = i.SalePrice,
                    //                        LocationCityID = i.LocationCityID,
                    //                        City = i.City,
                    //                        LocationStateID = i.LocationStateID,
                    //                        StateNM = i.StateNM,
                    //                        LocationCountryID = i.LocationCountryID,
                    //                        CountryNM = i.CountryNM,
                    //                        IsSold = i.IsSold,
                    //                        IsFeatured = i.IsFeatured
                    //                    }).ToList());
                }
                else if (MySQLFilter.Count < 1)
                {
                    //myResults.AddRange((from i in mycon.VwCollectionItems.OrderBy("SpecimenNumber")
                    //                    select new MineralCollectionItem()
                    //                    {
                    //                        CollectionItemID = i.CollectionItemID,
                    //                        CollectionID = i.CollectionID,
                    //                        CollectionNM = i.CollectionNM,
                    //                        SpecimenNumber = i.SpecimenNumber,
                    //                        ImageFileNM = i.ImageFileNM,
                    //                        Nickname = i.Nickname,
                    //                        PrimaryMineralID = i.PrimaryMineralID,
                    //                        PrimaryMineralNM = i.PrimaryMineralNM,
                    //                        MineralVariety = i.MineralVariety,
                    //                        MineNM = i.MineNM,
                    //                        PurchaseDate = i.PurchaseDate,
                    //                        PurchasePrice = i.PurchasePrice,
                    //                        Value = i.Value,
                    //                        ShowWherePurchased = i.ShowWherePurchased,
                    //                        PurchasedFromCompanyID = i.PurchasedFromCompanyID,
                    //                        PurchasedFromCompanyNM = i.CompanyNM,
                    //                        StorageLocation = i.StorageLocation,
                    //                        SpecimenNotes = i.SpecimenNotes,
                    //                        Description = i.Description,
                    //                        ExCollection = i.ExCollection,
                    //                        HeightCm = i.HeightCm,
                    //                        WidthCm = i.WidthCm,
                    //                        ThicknessCm = i.ThicknessCm,
                    //                        HeightIn = i.HeightIn,
                    //                        WidthIn = i.WidthIn,
                    //                        ThicknessIn = i.ThicknessIn,
                    //                        WeightGr = i.WeightGr,
                    //                        WeightKg = i.WeightKg,
                    //                        SaleDT = i.SaleDT,
                    //                        SalePrice = i.SalePrice,
                    //                        LocationCityID = i.LocationCityID,
                    //                        City = i.City,
                    //                        LocationStateID = i.LocationStateID,
                    //                        StateNM = i.StateNM,
                    //                        LocationCountryID = i.LocationCountryID,
                    //                        CountryNM = i.CountryNM,
                    //                        IsSold = i.IsSold,
                    //                        IsFeatured = i.IsFeatured
                    //                    }).ToList());
                }

                else
                {

                    //myResults.AddRange((from i in mycon.VwCollectionItems.Where(MySQLFilter.GetLINQWhere()).OrderBy("SpecimenNumber")
                    //                    select new MineralCollectionItem()
                    //                    {
                    //                        CollectionItemID = i.CollectionItemID,
                    //                        CollectionID = i.CollectionID,
                    //                        CollectionNM = i.CollectionNM,
                    //                        SpecimenNumber = i.SpecimenNumber,
                    //                        ImageFileNM = i.ImageFileNM,
                    //                        Nickname = i.Nickname,
                    //                        PrimaryMineralID = i.PrimaryMineralID,
                    //                        PrimaryMineralNM = i.PrimaryMineralNM,
                    //                        MineralVariety = i.MineralVariety,
                    //                        MineNM = i.MineNM,
                    //                        PurchaseDate = i.PurchaseDate,
                    //                        PurchasePrice = i.PurchasePrice,
                    //                        Value = i.Value,
                    //                        ShowWherePurchased = i.ShowWherePurchased,
                    //                        PurchasedFromCompanyID = i.PurchasedFromCompanyID,
                    //                        PurchasedFromCompanyNM = i.CompanyNM,
                    //                        StorageLocation = i.StorageLocation,
                    //                        SpecimenNotes = i.SpecimenNotes,
                    //                        Description = i.Description,
                    //                        ExCollection = i.ExCollection,
                    //                        HeightCm = i.HeightCm,
                    //                        WidthCm = i.WidthCm,
                    //                        ThicknessCm = i.ThicknessCm,
                    //                        HeightIn = i.HeightIn,
                    //                        WidthIn = i.WidthIn,
                    //                        ThicknessIn = i.ThicknessIn,
                    //                        WeightGr = i.WeightGr,
                    //                        WeightKg = i.WeightKg,
                    //                        SaleDT = i.SaleDT,
                    //                        SalePrice = i.SalePrice,
                    //                        LocationCityID = i.LocationCityID,
                    //                        City = i.City,
                    //                        LocationStateID = i.LocationStateID,
                    //                        StateNM = i.StateNM,
                    //                        LocationCountryID = i.LocationCountryID,
                    //                        CountryNM = i.CountryNM,
                    //                        IsSold = i.IsSold,
                    //                        IsFeatured = i.IsFeatured
                    //                    }).ToList());
                }
                if (MaxIndex < 0)
                {
                    ResultText = string.Format("No Results Found", CurrentIndex, MaxIndex);
                }
                else if (MaxIndex == 0)
                {
                    ResultText = string.Format("{1} Result Found", CurrentIndex + 1, MaxIndex + 1);
                    CurrentCollectionItemID = myResults[0].CollectionItemID;
                    GetCurrentItem();
                }
                else
                {
                    ResultText = string.Format("{1} Results Found", CurrentIndex + 1, MaxIndex + 1);
                }
            }
            catch (Exception ex)
            {
                // ApplicationLogging.ErrorLog("MineralCollectionListView.GetList  - multiple criteria", ex.ToString());
            }

            // If myResults.Count = 1 Then
            // If MySQLFilter.FindField("MineralID").Count > 0 Then
            // CurrentCollectionItemID = TryCast(myResults(0), vwMineralCollectionItem).CollectionItemID
            // Else
            // CurrentCollectionItemID = TryCast(myResults(0), vwCollectionItem).CollectionItemID
            // End If
            // myResults.Clear()
            // Dim sWhere = " CollectionItemID = " & CurrentCollectionItemID
            // myResults.AddRange((From i In mycon.CollectionItems.Where(sWhere).OrderBy("SpecimenNumber")).ToList())
            // End If

        }
        return myResults;

    }

}