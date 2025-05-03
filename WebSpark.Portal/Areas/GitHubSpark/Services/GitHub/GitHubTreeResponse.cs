using System.Text.Json.Serialization;

namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub
{
    public class GitHubTreeResponse
    {
        [JsonPropertyName("sha")]
        public string Sha { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("tree")]
        public List<GitHubTreeItem> Tree { get; set; } = new List<GitHubTreeItem>();

        [JsonPropertyName("truncated")]
        public bool Truncated { get; set; }
    }
}