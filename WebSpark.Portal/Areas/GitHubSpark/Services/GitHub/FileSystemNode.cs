namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Represents a node in a file system tree structure, used for representing GitHub repository content
/// </summary>
public class FileSystemNode
{
    /// <summary>
    /// Gets or sets the name of the file or directory
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full path of the file or directory within the repository
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the GitHub URL for the file or directory
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SHA hash of the file content in GitHub
    /// </summary>
    public string Sha { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this node represents a directory
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// Gets or sets the list of child nodes (for directories)
    /// </summary>
    public List<FileSystemNode>? Children { get; set; }

    /// <summary>
    /// Gets or sets the file type of this node
    /// </summary>
    public FileType FileType { get; set; }

    /// <summary>
    /// Gets or sets the content of the file (if loaded)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of class information found in this file (for code files)
    /// </summary>
    public List<ClassInfo> ClassInformationList { get; set; } = new List<ClassInfo>();
}