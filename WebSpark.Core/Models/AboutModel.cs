namespace WebSpark.Core.Models;

public class AboutModel
{
    public string Version { get; set; }
    public string OperatingSystem { get; set; }
    public string DatabaseProvider { get; set; }
    public BuildVersion Build { get; set; }
}

