namespace InquirySpark.Domain.SDK;

public class ApplicationUserRoleItem
{

    // Role Fields
    public int ApplicationUserRoleID { get; set; }
    public bool CanCreate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanManage { get; set; }
    public bool CanRead { get; set; }
    public bool CanReview { get; set; }
    public bool CanUpdate { get; set; }
    public int ReviewLevel { get; set; }
    public string RoleCD { get; set; }
    public string RoleDS { get; set; }
    public int RoleID { get; set; }
    public string RoleNM { get; set; }

    // User Fields
    public string AccountNM { get; set; }
    public int ApplicationUserID { get; set; }
    public string eMailAddress { get; set; }
    public string FirstNM { get; set; }
    public DateTime LastLoginDT { get; set; }
    public string LastLoginLocation { get; set; }
    public string LastNM { get; set; }

    // Application Fields
    public string ApplicationCD { get; set; }
    public string ApplicationDS { get; set; }
    public int ApplicationID { get; set; }
    public string ApplicationNM { get; set; }
    public string ApplicationShortNM { get; set; }
    public int ApplicationTypeID { get; set; }
    public string ApplicationTypeNM { get; set; }
    public string CommentDS { get; set; }
    public int MenuOrder { get; set; }
    public int ModifiedID { get; set; }
    // Company Fields
    public int CompanyID { get; set; }
    public string CompanyNM { get; set; }
    public string CompanyCD { get; set; }
    public bool IsDemo { get; set; }
    public DateTime StartupDate { get; set; }
    public bool isMonthlyPrice { get; set; }
    public decimal Price { get; set; }
    public bool UserInroled { get; set; }
    public bool IsUserAdmin { get; set; }
}