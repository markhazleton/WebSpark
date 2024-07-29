using System.Text.Json.Serialization;

namespace HttpClientUtility.Concurrent;

/// <summary>
/// Represents a build version.
/// </summary>
public record BuildVersion
{
    /// <summary>
    /// Gets or sets the major version.
    /// </summary>
    [JsonPropertyName("majorVersion")]
    public int? MajorVersion { get; set; }

    /// <summary>
    /// Gets or sets the minor version.
    /// </summary>
    [JsonPropertyName("minorVersion")]
    public int? MinorVersion { get; set; }

    /// <summary>
    /// Gets or sets the build number.
    /// </summary>
    [JsonPropertyName("build")]
    public int? Build { get; set; }

    /// <summary>
    /// Gets or sets the revision number.
    /// </summary>
    [JsonPropertyName("revision")]
    public int? Revision { get; set; }
}
