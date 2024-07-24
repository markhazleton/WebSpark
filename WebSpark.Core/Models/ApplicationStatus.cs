namespace WebSpark.Core.Models;

/// <summary>
/// 
/// </summary>
public class ApplicationStatus
{
    readonly Assembly _assembly;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly"></param>
    public ApplicationStatus(Assembly assembly)
    {
        _assembly = assembly;

        var oVer = _assembly?.GetName().Version;
        BuildDate = GetBuildDate(_assembly);
        BuildVersion = new Models.BuildVersion()
        {
            MajorVersion = oVer.Major,
            MinorVersion = oVer.Minor,
            Build = oVer.Build,
            Revision = oVer.Revision,
            BuildDate = GetBuildDate()
        };
    }

    private DateTime GetBuildDate()
    {
        const string BuildVersionMetadataPrefix = "+build";

        var attribute = _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix);
            if (index > 0)
            {
                value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }
        }
        return default;
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

    public DateTime BuildDate { get; }
    /// <summary>
    /// 
    /// </summary>
    public Models.BuildVersion BuildVersion { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Dictionary<string, string> Features { get; set; } = [];
    /// <summary>
    /// 
    /// </summary>
    public List<string> Messages { get; set; } = [];
    /// <summary>
    /// 
    /// </summary>
    public string Region { get; set; } = Environment.GetEnvironmentVariable("Region") ?? Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
    /// <summary>
    /// 
    /// </summary>
    public Models.ServiceStatus Status { get; set; } = Models.ServiceStatus.Online;
    /// <summary>
    /// 
    /// </summary>
    public Dictionary<string, string> Tests { get; set; } = [];
}
