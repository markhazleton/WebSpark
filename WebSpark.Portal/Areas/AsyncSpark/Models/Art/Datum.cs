namespace WebSpark.Portal.Areas.AsyncSpark.Models.Art
{
    public class Datum
    {
        public int id { get; set; }
        public string title { get; set; }
        public string image_id { get; set; }
        public string artist_title { get; set; }
        public List<string> material_titles { get; set; } = new List<string>();
        public string style_title { get; set; }
        public string artist_display { get; set; } // Additional details for artwork
        public string date_display { get; set; }
        public string dimensions { get; set; }
        public string medium_display { get; set; }
    }
}
