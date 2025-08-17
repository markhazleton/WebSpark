namespace WebSpark.Core.Models;

public class Section
{
    public string Label { get; set; } = string.Empty;
    public List<Field> Fields { get; set; } = [];
}
