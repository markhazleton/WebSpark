using Microsoft.Extensions.Logging;

namespace DocSpark;
/// <summary>
/// Provides services for file-related operations including retrieving source files,
/// writing to files, and building a file system tree.
/// </summary>
public class FileService
{
    private readonly ILogger<FileService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    /// <param name="logger">An instance of <see cref="ILogger{FileService}"/> for logging purposes.</param>
    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all C# source files in the specified project directory, excluding files in "bin" and "obj" folders.
    /// </summary>
    /// <param name="projectPath">The path to the project directory.</param>
    /// <returns>An array of file paths to C# source files, or an empty array if the path is invalid or empty.</returns>
    public string[] GetSourceFiles(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
        {
            _logger.LogError("Invalid project path: '{ProjectPath}'", projectPath);
            return Array.Empty<string>();
        }

        // Get all .cs files recursively, excluding bin and obj directories.
        return [.. Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories).Where(file => !file.Contains("\\bin\\") && !file.Contains("\\obj\\"))];
    }

    /// <summary>
    /// Writes the specified content to a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="content">The content to write to the file.</param>
    public async Task WriteToFileAsync(string filePath, string content)
    {
        try
        {
            await File.WriteAllTextAsync(filePath, content);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied to file: {FilePath}", filePath);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid file path: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while writing to file: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// Builds a hierarchical representation of the file system starting from the specified root directory.
    /// </summary>
    /// <param name="rootPath">The root directory path.</param>
    /// <returns>A <see cref="FileSystemNode"/> representing the directory structure, or null if the directory does not exist.</returns>
    public FileSystemNode? GetFileSystemTree(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            _logger.LogError("The directory '{RootPath}' does not exist.", rootPath);
            return null;
        }

        // Recursively build the file system tree.
        return BuildFileSystemTree(rootPath);
    }

    /// <summary>
    /// Recursively constructs a file system tree from the specified path, ignoring certain directories and files.
    /// </summary>
    /// <param name="path">The directory path to process.</param>
    /// <returns>A <see cref="FileSystemNode"/> representing the current directory and its contents.</returns>
    private FileSystemNode BuildFileSystemTree(string path)
    {
        var node = new FileSystemNode
        {
            Name = Path.GetFileName(path),
            Path = path,
            IsDirectory = Directory.Exists(path),
            Children = []
        };

        if (node.IsDirectory)
        {
            // Directories to ignore
            var excludedDirectories = new[] { "bin", "obj", "node_modules" };

            // Add all subdirectories as children, excluding ignored directories.
            foreach (var directory in Directory.GetDirectories(path)
                .Where(dir => !excludedDirectories.Contains(Path.GetFileName(dir), StringComparer.OrdinalIgnoreCase)))
            {
                node.Children.Add(BuildFileSystemTree(directory));
            }

            // Add all files as children, excluding non-source files.
            foreach (var file in Directory.GetFiles(path)
                .Where(file => file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
            {
                node.Children.Add(new FileSystemNode
                {
                    Name = Path.GetFileName(file),
                    Path = file,
                    IsDirectory = false,
                    Children = null
                });
            }
        }

        return node;
    }
}

/// <summary>
/// Represents a node in the file system tree, encapsulating details about files and directories.
/// </summary>
public class FileSystemNode
{
    /// <summary>
    /// Gets or sets the name of the file or directory.
    /// </summary>
    /// <remarks>
    /// For directories, this represents the folder name. For files, it represents the file name with extension.
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full path to the file or directory.
    /// </summary>
    /// <remarks>
    /// This is the absolute path, including the root drive or network location.
    /// </remarks>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the node represents a directory.
    /// </summary>
    /// <value>
    /// <c>true</c> if the node represents a directory; otherwise, <c>false</c>.
    /// </value>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// Gets or sets the list of child nodes. Null for files.
    /// </summary>
    /// <remarks>
    /// For directories, this contains the child directories and files. For files, this is always null.
    /// </remarks>
    public List<FileSystemNode>? Children { get; set; } = new();
}
