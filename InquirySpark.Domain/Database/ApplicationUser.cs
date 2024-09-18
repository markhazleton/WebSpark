namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents an application user.
/// </summary>
public partial class ApplicationUser
{
    /// <summary>
    /// Gets or sets the application user ID.
    /// </summary>
    public int ApplicationUserId { get; set; }

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    [DisplayName("First CompanyNm")]
    public string FirstNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    [DisplayName("Last CompanyNm")]
    public string LastNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [DisplayName("Email Address")]
    public string EMailAddress { get; set; } = null!;

    /// <summary>
    /// Gets or sets the comment description of the user.
    /// </summary>
    [DisplayName("Comment")]
    public string? CommentDs { get; set; }

    /// <summary>
    /// Gets or sets the account name of the user.
    /// </summary>
    [DisplayName("Account CompanyNm")]
    public string AccountNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the supervisor account name of the user.
    /// </summary>
    [DisplayName("Supervisor Account CompanyNm")]
    public string? SupervisorAccountNm { get; set; }

    /// <summary>
    /// Gets or sets the last login date and time of the user.
    /// </summary>
    [DisplayName("Last Login Date")]
    public DateTime? LastLoginDt { get; set; }

    /// <summary>
    /// Gets or sets the last login location of the user.
    /// </summary>
    [DisplayName("Last Login Location")]
    public string? LastLoginLocation { get; set; }

    /// <summary>
    /// Gets or sets the display name of the user.
    /// </summary>
    [DisplayName("Display CompanyNm")]
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the password of the user.
    /// </summary>
    [DisplayName("Password")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// Gets or sets the role ID of the user.
    /// </summary>
    [DisplayName("Role")]
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the user key.
    /// </summary>
    [DisplayName("User Key")]
    public Guid UserKey { get; set; }

    /// <summary>
    /// Gets or sets the user login.
    /// </summary>
    [DisplayName("User Login")]
    public string UserLogin { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the email is verified.
    /// </summary>
    [DisplayName("Email Verified")]
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Gets or sets the verification code.
    /// </summary>
    [DisplayName("Verification Code")]
    public string VerifyCode { get; set; } = null!;

    /// <summary>
    /// Gets or sets the company ID.
    /// </summary>
    [DisplayName("Company")]
    public int? CompanyId { get; set; }

    /// <summary>
    /// Gets or sets the modified ID.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the modified date and time.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the collection of application user roles.
    /// </summary>
    public virtual ICollection<ApplicationUserRole> ApplicationUserRoles { get; set; } = new List<ApplicationUserRole>();

    /// <summary>
    /// Gets or sets the company.
    /// </summary>
    public virtual Company? Company { get; set; }

    /// <summary>
    /// Gets or sets the site role.
    /// </summary>
    public virtual SiteRole Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of survey response states.
    /// </summary>
    public virtual ICollection<SurveyResponseState> SurveyResponseStates { get; set; } = new List<SurveyResponseState>();

    /// <summary>
    /// Gets or sets the collection of survey responses.
    /// </summary>
    public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();

    /// <summary>
    /// Gets or sets the collection of user application properties.
    /// </summary>
    public virtual ICollection<UserAppProperty> UserAppProperties { get; set; } = new List<UserAppProperty>();

    /// <summary>
    /// Gets or sets the collection of user messages from users.
    /// </summary>
    public virtual ICollection<UserMessage> UserMessageFromUsers { get; set; } = new List<UserMessage>();

    /// <summary>
    /// Gets or sets the collection of user messages to users.
    /// </summary>
    public virtual ICollection<UserMessage> UserMessageToUsers { get; set; } = new List<UserMessage>();
}
