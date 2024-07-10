using System.Text.Json.Serialization;

namespace HttpClientUtility.Concurrent;

public record BuildVersion(
 [property: JsonPropertyName("majorVersion")] int? MajorVersion,
 [property: JsonPropertyName("minorVersion")] int? MinorVersion,
 [property: JsonPropertyName("build")] int? Build,
 [property: JsonPropertyName("revision")] int? Revision
);
