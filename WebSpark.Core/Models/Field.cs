namespace WebSpark.Core.Models;

public class Field
{
    public string Id { get; set; }
    public string Label { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
    public List<string> Options { get; set; }
}
