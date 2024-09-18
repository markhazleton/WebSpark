namespace InquirySpark.Domain.SDK;

public partial class UserAppSetting
{
    public int UserAppSettingID { get; set; }
    public int UserID { get; set; }
    public int AppID { get; set; }
    public string Key { get; set; }
    public object Value { get; set; }
}