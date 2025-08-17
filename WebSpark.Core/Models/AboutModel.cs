namespace WebSpark.Core.Models;

public class AboutModel
{
    public string Version { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string DatabaseProvider { get; set; } = string.Empty;
    public BuildVersion Build { get; set; } = null!;
}

