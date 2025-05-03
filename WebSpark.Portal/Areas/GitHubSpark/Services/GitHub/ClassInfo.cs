namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Represents information about a class found in a C# file during repository analysis
/// </summary>
public class ClassInfo
{
    /// <summary>
    /// Gets or sets the name of the class
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the namespace containing the class
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the XML comment documentation for the class
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the name of the class that this class inherits from
    /// </summary>
    public string? InheritedClass { get; set; }

    /// <summary>
    /// Gets or sets the list of interfaces implemented by this class
    /// </summary>
    public List<string> ImplementedInterfaces { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the file path where the class is defined
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the route prefix for API controller classes
    /// </summary>
    public string? RoutePrefix { get; set; }

    /// <summary>
    /// Gets or sets the list of properties defined in the class
    /// </summary>
    public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();

    /// <summary>
    /// Gets or sets the list of methods defined in the class
    /// </summary>
    public List<MethodInfo> Methods { get; set; } = new List<MethodInfo>();

    /// <summary>
    /// Gets or sets a value indicating whether this class is decorated with the ApiController attribute
    /// </summary>
    public bool IsApiClass { get; set; }

    /// <summary>
    /// Gets or sets the root project path
    /// </summary>
    public string ProjectRoot { get; set; } = string.Empty;
}