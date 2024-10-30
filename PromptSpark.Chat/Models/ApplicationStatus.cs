
using System.Globalization;
using System.Reflection;

namespace PromptSpark.Chat.Models;

/// <summary>
/// ApplicationStatus
/// </summary>
public sealed class ApplicationStatus
{
    /// <summary>
    /// 
    /// </summary>
    public ApplicationStatus()
    {

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly"></param>
    public ApplicationStatus(Assembly assembly)
    {
        BuildDate = GetBuildDate(assembly);
        BuildVersion = new BuildVersion(assembly);
    }

    private DateTime GetBuildDate(Assembly assembly)
    {
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

    /// <summary>
    /// 
    /// </summary>
    public DateTime BuildDate { get; }
    /// <summary>
    /// BuildVersion
    /// </summary>
    public BuildVersion BuildVersion { get; }
    /// <summary>
    /// Features
    /// </summary>
    public Dictionary<string, string> Features { get; } = [];
    /// <summary>
    /// Messages
    /// </summary>
    public List<string> Messages { get; } = [];
    /// <summary>
    /// Region
    /// </summary>
    public string? Region { get; } = Environment.GetEnvironmentVariable("Region") ?? Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
    /// <summary>
    /// Status 
    /// </summary>
    public ServiceStatus Status { get; } = ServiceStatus.Online;
    /// <summary>
    /// Tests 
    /// </summary>
    public Dictionary<string, string> Tests { get; } = [];
}
