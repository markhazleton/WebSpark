namespace InquirySpark.Domain.SDK;

public class CompanyItem
{
    public int CompanyID { get; set; }
    public string CompanyNM { get; set; }
    public string CompanyCD { get; set; }
    public string CompanyDS { get; set; }
    public string Title { get; set; }
    public string SiteTheme { get; set; }
    public string DefaultSiteTheme { get; set; }
    public string GalleryFolder { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
    public string SiteURL { get; set; }
    public string FromEmail { get; set; }
    public string SMTP { get; set; }
    public string Component { get; set; }
    public int ModifiedID { get; set; }
    public DateTime ModifiedDT { get; set; }
    public bool Active { get; set; }
    public int ProjectCount { get; set; }
    public int UserCount { get; set; }
    public int SurveyResponseCount { get; set; }
    public List<ApplicationUserItem> UserList { get; set; } = [];
    public List<ApplicationItem> ProjectList { get; set; } = [];
}