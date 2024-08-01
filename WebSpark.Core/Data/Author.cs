namespace WebSpark.Core.Data;

public class Author : BaseEntity
{
    [Required]
    [EmailAddress]
    [StringLength(160)]
    public required string Email { get; set; }
    [Required]
    [StringLength(160)]
    public required string Password { get; set; }
    [Required]
    [StringLength(160)]
    public required string DisplayName { get; set; }
    [StringLength(2000)]
    public string? Bio { get; set; }
    [StringLength(400)]
    public string? Avatar { get; set; }
    public bool IsAdmin { get; set; }
    public List<Post> Posts { get; set; }
}
