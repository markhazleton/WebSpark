namespace WebSpark.Main.Areas.Async.Models.Art;

public class Pagination
{
    public int total { get; set; }                 // Total number of items
    public int limit { get; set; }                 // Number of items per page
    public int offset { get; set; }                // Offset from the start
    public int total_pages { get; set; }           // Total number of pages
    public int current_page { get; set; }          // Current page number
    public string next_url { get; set; }           // URL for the next page
}
public class ArtDetailsResponse
{
    public ArtData data { get; set; }      // Main data of the artwork
    public Info info { get; set; }         // Licensing information
    public Config config { get; set; }     // Configuration information
}

public class ArtData
{
    public int id { get; set; }                        // Artwork ID
    public string title { get; set; }                  // Title of the artwork
    public string date_display { get; set; }           // Display date of the artwork
    public string artist_display { get; set; }         // Artist display information (name, nationality, dates)
    public string dimensions { get; set; }             // Dimensions of the artwork
    public string medium_display { get; set; }         // Medium used in the artwork
    public string artist_title { get; set; }           // Artist's name
    public string style_title { get; set; }            // Style of the artwork
    public List<string> material_titles { get; set; }  // Materials used in the artwork
    public string image_id { get; set; }               // Image identifier

    // Computed property to generate the image URL using the IIIF URL from config
    public string ImageUrl(string iiifBaseUrl)
    {
        return $"{iiifBaseUrl}/{image_id}/full/843,/0/default.jpg";
    }
}

public class Info
{
    public string license_text { get; set; }           // License information text
    public List<string> license_links { get; set; }    // Links related to the license
    public string version { get; set; }                // Version of the API or data set
}

public class Config
{
    public string iiif_url { get; set; }               // Base URL for IIIF image API
    public string website_url { get; set; }            // Website URL for the Art Institute
}

