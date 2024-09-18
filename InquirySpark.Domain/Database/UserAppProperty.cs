namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a user application property.
/// </summary>
public partial class UserAppProperty
{
    /// <summary>
    /// Gets or sets the ID of the user application property.
    /// </summary>
    [DisplayName("ID")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user.
    /// </summary>
    [DisplayName("User ID")]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the application.
    /// </summary>
    [DisplayName("Application ID")]
    public int AppId { get; set; }

    /// <summary>
    /// Gets or sets the key of the user application property.
    /// </summary>
    [DisplayName("Key")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the value of the user application property.
    /// </summary>
    [DisplayName("Value")]
    public string Value { get; set; } = null!;

    /// <summary>
    /// Gets or sets the application associated with the user application property.
    /// </summary>
    [DisplayName("Application")]
    public virtual Application App { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user associated with the user application property.
    /// </summary>
    [DisplayName("User")]
    public virtual ApplicationUser User { get; set; } = null!;
}
