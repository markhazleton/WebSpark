namespace WebSpark.Core.Models
{
    public class SocialField : CustomField
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int Rank { get; set; }
    }
}
