namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents the role of an application user.
/// </summary>
public partial class ApplicationUserRole
{
    /// <summary>
    /// Gets or sets the ID of the application user role.
    /// </summary>
    [DisplayName("Application User Role ID")]
    public int ApplicationUserRoleId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the application.
    /// </summary>
    [DisplayName("Application ID")]
    public int ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the application user.
    /// </summary>
    [DisplayName("Application User ID")]
    public int ApplicationUserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the role.
    /// </summary>
    [DisplayName("Role ID")]
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the modifier.
    /// </summary>
    [DisplayName("Modifier ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the date and time of modification.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is in demo mode.
    /// </summary>
    [DisplayName("Is Demo")]
    public bool? IsDemo { get; set; }

    /// <summary>
    /// Gets or sets the startup date.
    /// </summary>
    [DisplayName("Startup Date")]
    public DateTime? StartUpDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the price is monthly.
    /// </summary>
    [DisplayName("Is Monthly Price")]
    public bool? IsMonthlyPrice { get; set; }

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    [DisplayName("Price")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is enrolled.
    /// </summary>
    [DisplayName("User Enrolled")]
    public bool? UserInRolled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is an admin.
    /// </summary>
    [DisplayName("Is User Admin")]
    public bool? IsUserAdmin { get; set; }

    /// <summary>
    /// Gets or sets the associated application.
    /// </summary>
    [DisplayName("Application")]
    public virtual Application Application { get; set; } = null!;

    /// <summary>
    /// Gets or sets the associated application user.
    /// </summary>
    [DisplayName("Application User")]
    public virtual ApplicationUser ApplicationUser { get; set; } = null!;

    /// <summary>
    /// Gets or sets the associated role.
    /// </summary>
    [DisplayName("Role")]
    public virtual Role Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of survey response answer reviews.
    /// </summary>
    [DisplayName("Survey Response Answer Reviews")]
    public virtual ICollection<SurveyResponseAnswerReview> SurveyResponseAnswerReviews { get; set; } = new List<SurveyResponseAnswerReview>();
}
