using WebSpark.Main.Areas.Async.Controllers;

namespace WebSpark.Main.Areas.Async.Models.Art;

public class ArtWorksResponse
{
    public Datum[]? data { get; set; }
    public Pagination? pagination { get; set; } // Optional, if you need pagination details
}
