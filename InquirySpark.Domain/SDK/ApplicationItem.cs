namespace InquirySpark.Domain.SDK;

public class ApplicationItem
{
    public int ApplicationID { get; set; }
    public string ApplicationNM { get; set; }
    public string ApplicationCD { get; set; }
    public string ApplicationDS { get; set; }
    public string ApplicationShortNM { get; set; }
    public int ApplicationTypeID { get; set; }
    public int CompanyID { get; set; }
    public string CompanyNM { get; set; }
    public int MenuOrder { get; set; }
    public string ApplicationTypeNM { get; set; }
    public List<ApplicationUserRoleItem> ApplicationUserList { get; set; } = [];
    public List<ApplicationSurveyItem> ApplicationSurveyList { get; set; } = [];
    public DateTime ModifiedDT { get; set; }
    public int ModifiedID { get; set; }
    public int SurveyCount { get; set; }
    public int UserCount { get; set; }
    public int SurveyResponseCount { get; set; }
    public string ApplicationFolder { get; set; }
    public int DefaultAppPage { get; set; }
    public List<PropertyItem> Properties { get; set; } = [];
    public List<NavigationMenuItem> Navigation { get; set; } = [];
}