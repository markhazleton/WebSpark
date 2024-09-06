namespace WebSpark.Portal.Areas.AsyncSpark.Models.Nasa;
using System.Text.Json.Serialization;

public record NasaPictureDto(
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("explanation")] string Explanation,
    [property: JsonPropertyName("hdurl")] string HdUrl,
    [property: JsonPropertyName("media_type")] string MediaType,
    [property: JsonPropertyName("service_version")] string ServiceVersion,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("copyright")] string Copyright
);
public class NasaPictureListDto : List<NasaPictureDto>
{
}