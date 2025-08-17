namespace WebSpark.Core.Models;

public class PostItem
{
    public int Id { get; set; }
    public PostType PostType { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Required]
    public string Content { get; set; } = string.Empty;
    public ICollection<CategoryItem> Categories { get; set; } = new List<CategoryItem>();
    public string Cover { get; set; } = string.Empty;
    public int PostViews { get; set; }
    public double Rating { get; set; }
    public DateTime Published { get; set; }
    public bool IsPublished { get { return Published > DateTime.MinValue; } }
    public bool Featured { get; set; }

    public AuthorItem Author { get; set; } = new();
    public SaveStatus Status { get; set; }
    public List<SocialField> SocialFields { get; set; } = [];
    public bool Selected { get; set; }

    // to be able compare two posts
    // if(post1 == post2) { ... }
    public bool Equals(Models.PostItem? other)
    {
        if (other is null) return false;
        if (Id == other.Id)
            return true;

        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
