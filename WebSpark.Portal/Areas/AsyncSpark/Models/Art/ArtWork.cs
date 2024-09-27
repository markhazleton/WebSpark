namespace WebSpark.Portal.Areas.AsyncSpark.Models.Art
{
    public class ArtWork
    {
        public ArtWork()
        {
                
        }
        public ArtWork(ArtData? artData)
        {
            if (artData == null)
            {
                return;
            }
            id = artData.id.ToString();
            title = artData.title;
            image_id = artData.image_id;
            artist_title = artData.artist_title;
            material_titles = artData.material_titles;
            style_title = artData.style_title;
            artist_display = artData.artist_display;
            date_display = artData.date_display;
            dimensions = artData.dimensions;
            medium_display = artData.medium_display;
        }
        public ArtWork(Datum? datum)
        {
            if (datum == null)
            {
                return;
            }
            id = datum.id.ToString();
            title = datum.title;
            image_id = datum.image_id;
            artist_title = datum.artist_title;
            material_titles = datum.material_titles;
            style_title = datum.style_title;
            artist_display = datum.artist_display;
            date_display = datum.date_display;
            dimensions = datum.dimensions;
            medium_display = datum.medium_display;
        }
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
