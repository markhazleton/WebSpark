namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a role in the system.
/// </summary>
public partial class Role
{
    /// <summary>
    /// Gets or sets the role ID.
    /// </summary>
    [DisplayName("Role ID")]
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the role code.
    /// </summary>
    [DisplayName("Role Code")]
    public string RoleCd { get; set; } = null!;

    /// <summary>
    /// Gets or sets the role name.
    /// </summary>
    [DisplayName("Role CompanyNm")]
    public string RoleNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the role description.
    /// </summary>
    [DisplayName("Role Description")]
    public string RoleDs { get; set; } = null!;

    /// <summary>
    /// Gets or sets the review level.
    /// </summary>
    [DisplayName("Review Level")]
    public int ReviewLevel { get; set; }

    /// <summary>
    /// Gets or sets the read flag.
    /// </summary>
    [DisplayName("Read Flag")]
    public bool ReadFl { get; set; }

    /// <summary>
    /// Gets or sets the update flag.
    /// </summary>
    [DisplayName("Update Flag")]
    public bool UpdateFl { get; set; }

    /// <summary>
    /// Gets or sets the modified ID.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the modified date.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the application surveys associated with the role.
    /// </summary>
    [DisplayName("Application Surveys")]
    public virtual ICollection<ApplicationSurvey> ApplicationSurveys { get; set; } = [];

    /// <summary>
    /// Gets or sets the application user roles associated with the role.
    /// </summary>
    [DisplayName("Application User Roles")]
    public virtual ICollection<ApplicationUserRole> ApplicationUserRoles { get; set; } = [];
}
