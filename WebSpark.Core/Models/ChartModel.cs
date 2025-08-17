namespace WebSpark.Core.Models;

public class BarChartModel
{
    public ICollection<string> Labels { get; set; } = new List<string>();
    public ICollection<int> Data { get; set; } = new List<int>();
}
