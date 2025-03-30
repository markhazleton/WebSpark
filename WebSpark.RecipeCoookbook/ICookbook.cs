namespace WebSpark.RecipeCookbook;

/// <summary>
/// Interface for generating recipe cookbooks
/// </summary>
public interface ICookbook
{
    /// <summary>
    /// Creates a recipe cookbook PDF file with the specified parameters
    /// </summary>
    /// <param name="outputPath">Path where the PDF will be saved</param>
    /// <param name="name">Title of the cookbook</param>
    /// <param name="description">Description of the cookbook</param>
    /// <returns>The path to the created PDF file, or null if an error occurred</returns>
    string? MakeCookbook(string outputPath, string name, string description);
}