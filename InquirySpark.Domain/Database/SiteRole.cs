namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a site role.
/// </summary>
public partial class SiteRole
{
    /// <summary>
    /// Gets or sets the ID of the site role.
    /// </summary>
    [DisplayName("ID")]
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the site role.
    /// </summary>
    [DisplayName("Role CompanyNm")]
    public string RoleName { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the site role is active.
    /// </summary>
    [DisplayName("Active")]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the collection of application users associated with the site role.
    /// </summary>
    [DisplayName("Application Users")]
    public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; } = new List<ApplicationUser>();
}
