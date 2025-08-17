namespace WebSpark.Core.Models;

public class ImportModel
{
    [Required]
    [Url]
    public string FeedUrl { get; set; } = string.Empty;
    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;
    List<string> FileExtensions { get; set; } = ["zip", "7z", "xml", "pdf", "doc", "docx", "xls", "xlsx", "mp3", "mp4", "avi"];
}
