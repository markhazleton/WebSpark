namespace WebSpark.Core.Data;

public class Subscriber : BaseEntity
{
    [Required]
    [EmailAddress]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;
    [StringLength(80)]
    public string Ip { get; set; } = string.Empty;
    [StringLength(120)]
    public string Country { get; set; } = string.Empty;
    [StringLength(120)]
    public string Region { get; set; } = string.Empty;
    public Blog Blog { get; set; } = null!;
}
