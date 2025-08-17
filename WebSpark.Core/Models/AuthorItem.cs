namespace WebSpark.Core.Models;

public class AuthorItem
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;
    [Required]
    [StringLength(160)]
    public string Password { get; set; } = string.Empty;
    [Required]
    [StringLength(160)]
    public string DisplayName { get; set; } = string.Empty;
    [StringLength(2000)]
    public string Bio { get; set; } = string.Empty;
    [StringLength(400)]
    public string Avatar { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }

    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }

    public List<Models.PostItem> Posts { get; set; } = new();
    public bool Selected { get; set; }
}
