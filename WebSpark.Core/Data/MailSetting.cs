namespace WebSpark.Core.Data;

public class MailSetting : BaseEntity
{

    [Required]
    [StringLength(160)]
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string UserEmail { get; set; } = string.Empty;
    [Required]
    [StringLength(120)]
    public string UserPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string FromName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string FromEmail { get; set; } = string.Empty;
    [Required]
    [StringLength(120)]
    public string ToName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public Blog Blog { get; set; } = null!;
}
