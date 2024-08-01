namespace WebSpark.Core.Data;

public class MailSetting : BaseEntity
{

    [Required]
    [StringLength(160)]
    public string Host { get; set; }
    public int Port { get; set; }
    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string UserEmail { get; set; }
    [Required]
    [StringLength(120)]
    public string UserPassword { get; set; }

    [Required]
    [StringLength(120)]
    public string FromName { get; set; }
    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string FromEmail { get; set; }
    [Required]
    [StringLength(120)]
    public string ToName { get; set; }
    public bool Enabled { get; set; }
    public Blog Blog { get; set; }
}
