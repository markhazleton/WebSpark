using System.Text.Json.Serialization;

namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub
{
    public class FileContent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("encoding")]
        public string Encoding { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("sha")]
        public string Sha { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("git_url")]
        public string GitUrl { get; set; } = string.Empty;

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;

        [JsonPropertyName("download_url")]
        public string DownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets the decoded content of the file if it's Base64 encoded
        /// </summary>
        [JsonIgnore]
        public string DecodedContent
        {
            get
            {
                if (string.IsNullOrEmpty(Content) || Encoding.ToLowerInvariant() != "base64")
                    return Content;

                try
                {
                    byte[] data = Convert.FromBase64String(Content.Replace("\n", ""));
                    return System.Text.Encoding.UTF8.GetString(data);
                }
                catch
                {
                    return Content;
                }
            }
        }
    }
}