namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a user message.
/// </summary>
public partial class UserMessage
{
    /// <summary>
    /// Gets or sets the ID of the user message.
    /// </summary>
    [DisplayName("ID")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user receiving the message.
    /// </summary>
    [DisplayName("To User ID")]
    public int? ToUserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user sending the message.
    /// </summary>
    [DisplayName("From User ID")]
    public int? FromUserId { get; set; }

    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    [DisplayName("Message")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message has been opened.
    /// </summary>
    [DisplayName("Opened")]
    public bool? Opened { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time of the message.
    /// </summary>
    [DisplayName("Created Date Time")]
    [Column(name: "CratedDateTime")]
    public DateTime? CreatedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the subject of the message.
    /// </summary>
    [DisplayName("Subject")]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message has been deleted.
    /// </summary>
    [DisplayName("Deleted")]
    public bool? Deleted { get; set; }

    /// <summary>
    /// Gets or sets the ID of the application associated with the message.
    /// </summary>
    [DisplayName("App ID")]
    public int? AppId { get; set; }

    /// <summary>
    /// Gets or sets the page on which the message should be shown.
    /// </summary>
    [DisplayName("Show on Page")]
    public int? ShowonPage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message is from an application.
    /// </summary>
    [DisplayName("From App")]
    public bool? FromApp { get; set; }

    /// <summary>
    /// Gets or sets the user who sent the message.
    /// </summary>
    [DisplayName("From User")]
    public virtual ApplicationUser? FromUser { get; set; }

    /// <summary>
    /// Gets or sets the user who received the message.
    /// </summary>
    [DisplayName("To User")]
    public virtual ApplicationUser? ToUser { get; set; }
}
