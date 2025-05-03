namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Represents the type of file in a GitHub repository
/// </summary>
public enum FileType
{
    /// <summary>
    /// Unknown file type
    /// </summary>
    Unknown,

    /// <summary>
    /// Plain text file
    /// </summary>
    Text,

    /// <summary>
    /// Markdown document
    /// </summary>
    Markdown,

    /// <summary>
    /// Image file (jpg, png, gif, etc.)
    /// </summary>
    Image,

    /// <summary>
    /// JSON file
    /// </summary>
    Json,

    /// <summary>
    /// XML file
    /// </summary>
    Xml,

    /// <summary>
    /// HTML file
    /// </summary>
    Html,

    /// <summary>
    /// CSS file
    /// </summary>
    Css,

    /// <summary>
    /// JavaScript file
    /// </summary>
    Js,

    /// <summary>
    /// Source code file (cs, cpp, py, java, etc.)
    /// </summary>
    Code
}