using System.ComponentModel.DataAnnotations;

namespace WebSpark.Core.Data;

public class Author : BaseEntity
{
    public Author() { }

    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(160)]
    public string Email { get; set; }
    [Required]
    [StringLength(160)]
    public string Password { get; set; }
    [Required]
    [StringLength(160)]
    public string DisplayName { get; set; }
    [StringLength(2000)]
    public string Bio { get; set; }
    [StringLength(400)]
    public string Avatar { get; set; }
    public bool IsAdmin { get; set; }

    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }

    public List<Post> Posts { get; set; }
}
