namespace WebSpark.Core.Models;

public class ThemeItem
{
    public string Title { get; set; } = string.Empty;
    public string Cover { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public bool HasSettings { get; set; }
}
