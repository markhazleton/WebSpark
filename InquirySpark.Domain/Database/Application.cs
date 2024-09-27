namespace InquirySpark.Domain.Database;


/// <summary>
/// Application
/// </summary>
[DisplayColumn("ApplicationNm")]
public partial class Application
{
    /// <summary>
    /// Application Id (Primary Key)
    /// </summary>
    [Key]
    public int ApplicationId { get; set; }
    /// <summary>
    /// CompanyName of the application
    /// </summary>
    [Required]
    [Display(Name = "CompanyName")]
    public string ApplicationNm { get; set; } = null!;
    /// <summary>
    /// Code of the application (used for the naming child objects)
    /// </summary>
    [Display(Name = "Code")]
    public string ApplicationCd { get; set; } = null!;
    /// <summary>
    /// Short name of the application
    /// </summary>
    [Display(Name = "Short CompanyName")]
    public string ApplicationShortNm { get; set; } = null!;
    /// <summary>
    /// Lookup Id for the application type
    /// </summary>
    [Display(Name = "Type")]
    public int ApplicationTypeId { get; set; }
    /// <summary>
    /// Description of the application
    /// </summary>
    [Display(Name = "Description")]
    public string? ApplicationDs { get; set; }
    /// <summary>
    /// Display order of the application in the menu
    /// </summary>
    [Display(Name = "Menu Order")]
    public int MenuOrder { get; set; }

    /// <summary>
    /// Folder for Application Items
    /// </summary>
    [Display(Name = "Folder")]
    public string ApplicationFolder { get; set; } = null!;

    /// <summary>
    /// Default page for the application
    /// </summary>
    [Display(Name = "Default Page")]
    public int DefaultPageId { get; set; }

    public int? CompanyId { get; set; }

    public int ModifiedId { get; set; }

    public DateTime ModifiedDt { get; set; }

    public virtual ICollection<AppProperty> AppProperties { get; set; } = [];

    public virtual ICollection<ApplicationSurvey> ApplicationSurveys { get; set; } = [];

    public virtual LuApplicationType ApplicationType { get; set; } = null!;

    public virtual ICollection<ApplicationUserRole> ApplicationUserRoles { get; set; } = [];
    /// <summary>
    /// Company Lookup
    /// </summary>
    [Display(Name = "Company Lookup")]
    public virtual Company? Company { get; set; }

    public virtual ICollection<SiteAppMenu> SiteAppMenus { get; set; } = [];

    public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = [];

    public virtual ICollection<UserAppProperty> UserAppProperties { get; set; } = [];
}
