namespace WebSpark.Core.Data;

public class Subscriber : BaseEntity
{
    [Required]
    [EmailAddress]
    [StringLength(160)]
    public string Email { get; set; }
    [StringLength(80)]
    public string Ip { get; set; }
    [StringLength(120)]
    public string Country { get; set; }
    [StringLength(120)]
    public string Region { get; set; }
    public Blog Blog { get; set; }
}
