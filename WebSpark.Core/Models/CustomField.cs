namespace WebSpark.Core.Models;

public class CustomField
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
