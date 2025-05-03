namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Represents information about a method found in a C# class during repository analysis
/// </summary>
public class MethodInfo
{
    /// <summary>
    /// Gets or sets the name of the method
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the return type of the method
    /// </summary>
    public string ReturnType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of parameters for the method
    /// </summary>
    public List<string> Parameters { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the XML comment documentation for the method
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this method is public
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Gets or sets the route for API controller methods
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this method is an API endpoint
    /// </summary>
    public bool IsEndPoint { get; set; }
}