
namespace WebSpark.Portal.Areas.AsyncSpark.Models.Art;

public class ArtWorksResponse
{
    public Datum[]? data { get; set; }
    public Pagination? pagination { get; set; } // Optional, if you need pagination details
}
