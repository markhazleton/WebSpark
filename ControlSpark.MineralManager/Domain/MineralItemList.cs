using ControlSpark.Domain.Models.SQLHelper;
using ControlSpark.MineralManager.Entities;

namespace ControlSpark.MineralManager.Domain;

public class MineralItemListView
{
    public List<MineralItem> myResults { get; set; } = new List<MineralItem>();
    private const int INT_IndexBase = 0;
    public int CurrentIndex { get; set; }
    public int MaxIndex
    {
        get
        {
            return myResults.Count - 1;
        }
    }
    public SQLFilterList MySQLFilter { get; set; } = new SQLFilterList();

    public List<MineralItem> GetList()
    {
        myResults.Clear();
        using (var mycon = new MineralDbContext())
        {
            try
            {
                var myObjects = new List<object>();
                myObjects.AddRange((from i in mycon.Minerals
                                    select new { i.MineralId, i.MineralNm, i.MineralDs, i.ModifiedDt, i.ModifiedId, i.WikipediaUrl, PrimaryMineralCount = i.CollectionItems.Count, SecondaryMineralCount = i.CollectionItemMinerals.Count }).ToList());
            }

            catch (Exception ex)
            {
                // ApplicationLogging.ErrorLog("MineralItemListView.GetList", ex.ToString());
            }
        }
        return myResults;
    }
}

public class MineralItemList : List<MineralItem>
{

}