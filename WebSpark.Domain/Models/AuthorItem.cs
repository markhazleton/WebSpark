using WebSpark.Domain.Entities;

namespace WebSpark.Domain.Models;

public class AuthorItem
{
    public Author Author { get; set; }
    public bool Selected { get; set; }
}
