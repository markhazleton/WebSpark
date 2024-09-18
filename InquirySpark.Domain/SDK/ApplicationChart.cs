namespace InquirySpark.Domain.SDK;

/// <summary>
/// 
/// </summary>
public class ApplicationChartItem
{
    /// <summary>
    /// 
    /// </summary>
    public int ApplicationChartId { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int SiteUserID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int SiteAppID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string SettingType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string SettingName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string SettingValue { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string SettingValueEnhanced { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public DateTime DateCreated { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public DateTime LastUpdated { get; set; }
}