
namespace ControlSpark.Domain.Models;

public class BarChartModel
{
    public ICollection<string> Labels { get; set; }
    public ICollection<int> Data { get; set; }
}
