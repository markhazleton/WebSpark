namespace WebSpark.Portal.Areas.AsyncSpark.Models.Art
{
    public class ArtWork
    {
        public string id { get; set; }
        public string title { get; set; }
        public string image_id { get; set; }
        public string artist_title { get; set; }
        public List<string> material_titles { get; set; } = [];
        public string style_title { get; set; }
        public string artist_display { get; set; }
        public string date_display { get; set; }
        public string dimensions { get; set; }
        public string medium_display { get; set; }

        public string ImageUrl => $"https://www.artic.edu/iiif/2/{image_id}/full/843,/0/default.jpg";
    }
}
