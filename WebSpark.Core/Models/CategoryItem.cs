namespace WebSpark.Core.Models;

public class CategoryItem : IComparable<Models.CategoryItem>
{
    public bool Selected { get; set; }
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public DateTime DateCreated { get; set; }

    public int CompareTo(Models.CategoryItem? other)
    {
        if (other is null) return 1;
        return Category.ToLower().CompareTo(other.Category.ToLower());
    }
}
