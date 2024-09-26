using System.Reflection;
using System.Text.Json.Serialization;

namespace WebSpark.Core.Models;

public class ApplicationStatus
{
    [JsonPropertyName("buildDate")]
    public DateTime BuildDate { get; set; }

    [JsonPropertyName("buildVersion")]
    public BuildVersion BuildVersion { get; set; } = new BuildVersion();

    [JsonPropertyName("features")]
    public Dictionary<string, string> Features { get; set; } = [];

    [JsonPropertyName("messages")]
    public List<string> Messages { get; set; } = [];

    [JsonPropertyName("region")]
    public string Region { get; set; } = Environment.GetEnvironmentVariable("Region") ?? Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");

    [JsonPropertyName("status")]
    public ServiceStatus Status { get; set; } = ServiceStatus.Online;

    [JsonPropertyName("tests")]
    public Dictionary<string, string> Tests { get; set; } = [];

    public ApplicationStatus() { }

    public ApplicationStatus(Assembly assembly)
    {

        var oVer = assembly?.GetName().Version;
        BuildDate = GetBuildDate(assembly);
        BuildVersion = new BuildVersion()
        {
            MajorVersion = oVer?.Major ?? 0,
            MinorVersion = oVer?.Minor ?? 0,
            Build = oVer?.Build ?? 0,
            Revision = oVer?.Revision ?? 0,
            BuildDate = BuildDate
        };
    }

    private static DateTime GetBuildDate(Assembly? assembly)
    {
        if (assembly == null) return DateTime.MinValue;

        const string BuildVersionMetadataPrefix = "+build";
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix);
            if (index > 0)
            {
                value = value[(index + BuildVersionMetadataPrefix.Length)..];
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }
        }
        return DateTime.MinValue;
    }
}
