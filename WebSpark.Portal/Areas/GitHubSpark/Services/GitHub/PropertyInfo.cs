namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Represents information about a property found in a C# class during repository analysis
/// </summary>
public class PropertyInfo
{
    /// <summary>
    /// Gets or sets the name of the property
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the property
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the XML comment documentation for the property
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this property is public
    /// </summary>
    public bool IsPublic { get; set; }
}