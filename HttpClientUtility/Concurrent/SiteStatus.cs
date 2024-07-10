using System.Text.Json.Serialization;

namespace HttpClientUtility.Concurrent;

public record SiteStatus(
[property: JsonPropertyName("buildDate")] DateTime? BuildDate,
[property: JsonPropertyName("buildVersion")] BuildVersion BuildVersion,
[property: JsonPropertyName("features")] Features Features,
[property: JsonPropertyName("messages")] IReadOnlyList<object> Messages,
[property: JsonPropertyName("region")] string Region,
[property: JsonPropertyName("status")] int? Status,
[property: JsonPropertyName("tests")] Tests Tests
);
