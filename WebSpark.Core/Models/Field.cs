namespace WebSpark.Core.Models;

public class Field
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}
