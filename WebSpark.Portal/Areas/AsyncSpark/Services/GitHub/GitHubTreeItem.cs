namespace WebSpark.Portal.Areas.AsyncSpark.Services.GitHub;


/// <summary>
/// Represents an individual item in the GitHub tree API response.
/// </summary>
public class GitHubTreeItem
{
    /// <summary>
    /// The path of the item relative to the repository root.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// The file mode of the item (e.g., "100644" for file, "040000" for directory).
    /// </summary>
    public string Mode { get; set; }

    /// <summary>
    /// The type of the item ("blob" for files, "tree" for directories).
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// The size of the item in bytes (only applicable for blobs).
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// The SHA identifier of the item.
    /// </summary>
    public string Sha { get; set; }

    /// <summary>
    /// The URL of the item in the GitHub API.
    /// </summary>
    public string Url { get; set; }
}
