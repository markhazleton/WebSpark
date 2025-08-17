namespace WebSpark.Core.Models;

/// <summary>
/// Build Version
/// </summary>
public sealed class BuildVersion
{
    [JsonPropertyName("buildDate")]
    public DateTime BuildDate { get; set; }

    [JsonPropertyName("build")]
    public int Build { get; set; }

    [JsonPropertyName("majorVersion")]
    public int MajorVersion { get; set; }

    [JsonPropertyName("minorVersion")]
    public int MinorVersion { get; set; }

    [JsonPropertyName("revision")]
    public int Revision { get; set; }

    public BuildVersion()
    {

    }

    /// <summary>
    /// Build Version
    /// </summary>
    /// <param name="assembly"></param>
    public BuildVersion(Assembly? assembly)
    {
        var oVer = assembly?.GetName().Version;
        MajorVersion = oVer?.Major ?? 0;
        MinorVersion = oVer?.Minor ?? 0;
        Build = oVer?.Build ?? 0;
        Revision = oVer?.Revision ?? 0;
        BuildDate = assembly == null ? DateTime.MinValue : GetBuildDate(assembly);
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
    /// Override the To String Function to Format Version
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Version: {MajorVersion}.{MinorVersion}.{Build}.{Revision}";
    }
}

