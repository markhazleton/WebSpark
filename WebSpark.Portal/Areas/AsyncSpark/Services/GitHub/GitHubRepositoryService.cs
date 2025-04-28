using HttpClientUtility.MemoryCache;
using HttpClientUtility.RequestResult;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net;
using System.Text.Json.Serialization;

namespace WebSpark.Portal.Areas.AsyncSpark.Services.GitHub;

public class FileContent
{

    /// <summary>
    /// Decodes the Base64-encoded content to a plain text string.
    /// </summary>
    [JsonIgnore]
    public string DecodedContent
    {
        get
        {
            if (Encoding?.ToLowerInvariant() == "base64" && !string.IsNullOrEmpty(EncodedContent))
            {
                ReadOnlySpan<byte> decodedBytes = Convert.FromBase64String(EncodedContent);
                return System.Text.Encoding.UTF8.GetString(decodedBytes); // Fully qualified Encoding
            }
            return string.Empty;
        }
    }

    [JsonPropertyName("content")]
    public string EncodedContent { get; set; }

    [JsonPropertyName("encoding")]
    public string Encoding { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }
    [JsonPropertyName("sha")]
    public string Sha { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}
public class MethodInfo
{
    public string Comment { get; set; }
    public bool IsEndPoint { get; set; }
    public bool IsPublic { get; set; }
    public string Name { get; set; }
    public List<string> Parameters { get; set; }
    public string ReturnType { get; set; }
    public string Route { get; set; }
}
public class PropertyInfo
{
    public string Comment { get; set; }
    public bool IsPublic { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
}
public class ClassInfo
{

    public string RelativePath()
    {
        return Path.GetRelativePath(ProjectRoot, FilePath).Replace('\\', '/');
    }

    public string Comment { get; set; }
    public string FilePath { get; set; }
    public List<string> ImplementedInterfaces { get; set; }
    public string InheritedClass { get; set; }
    public bool IsApiClass { get; set; }
    public List<MethodInfo> Methods { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public string ProjectRoot { get; set; }
    public List<PropertyInfo> Properties { get; set; }
    public string RoutePrefix { get; set; }
}
public enum FileType
{
    Unknown,
    Text,
    Markdown,
    Image,
    Code,
    Json,
    Xml,
    Html,
    Css,
    Js,
    Binary
}
public class FileSystemNode
{

    /// <summary>
    /// Gets or sets the list of child nodes. Null for files.
    /// </summary>
    /// <remarks>
    /// For directories, this contains the child directories and files. For files, this is always null.
    /// </remarks>
    public List<FileSystemNode> Children { get; set; }
    public List<ClassInfo> ClassInformationList { get; set; } = [];
    public string Content { get; set; }
    public FileType FileType { get; set; } = FileType.Unknown;
    /// <summary>
    /// Gets or sets a value indicating whether the node represents a directory.
    /// </summary>
    /// <value>
    /// <c>true</c> if the node represents a directory; otherwise, <c>false</c>.
    /// </value>
    public bool IsDirectory { get; set; }
    /// <summary>
    /// Gets or sets the name of the file or directory.
    /// </summary>
    /// <remarks>
    /// For directories, this represents the folder name. For files, it represents the file name with extension.
    /// </remarks>
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the full path to the file or directory.
    /// </summary>
    /// <remarks>
    /// This is the absolute path, including the root drive or network location.
    /// </remarks>
    public string Path { get; set; }
    public string Sha { get; internal set; }
    public string Url { get; set; }
}
public class GitHubTreeResponse
{
    [JsonPropertyName("sha")]
    public string Sha { get; set; }

    [JsonPropertyName("tree")]
    public List<GitTreeNode> Tree { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}
public class GitTreeNode
{

    [JsonPropertyName("mode")]
    public string Mode { get; set; }
    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("sha")]
    public string Sha { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}



public class GitHubRepositoryService
{
    private const int MaxRepoLookupsPerHour = 50;

    private readonly IHttpRequestResultService _httpRequestResultService;
    private readonly IMemoryCacheManager _memoryCacheManager;
    private readonly string _repoLookupListCacheKey = "repo-lookup-list";
    private readonly string _token;

    public GitHubRepositoryService(IHttpRequestResultService httpClientSendService, IMemoryCacheManager memoryCacheManager, string token)
    {
        _httpRequestResultService = httpClientSendService;
        _memoryCacheManager = memoryCacheManager;
        _token = token;
    }

    private string AdjustRoutePrefix(string routePrefix, string className)
    {
        if (string.IsNullOrWhiteSpace(routePrefix) || !routePrefix.Contains("[controller]", StringComparison.OrdinalIgnoreCase))
            return routePrefix;

        var controllerBaseName = className.EndsWith("Controller")
            ? className.Substring(0, className.Length - "Controller".Length).ToLowerInvariant()
            : className.ToLowerInvariant();

        return routePrefix.Replace("[controller]", controllerBaseName, StringComparison.OrdinalIgnoreCase);
    }

    private FileSystemNode BuildFileSystemTreeFromGitHub(GitHubTreeResponse treeResponse)
    {
        var rootNode = new FileSystemNode
        {
            Name = "root",
            Url = treeResponse.Url,
            Path = string.Empty,
            Sha = treeResponse.Sha,
            IsDirectory = true,
            Children = []
        };

        var directoryMap = new Dictionary<string, FileSystemNode>
    {
        { string.Empty, rootNode } // Root node
    };

        foreach (var item in treeResponse.Tree)
        {
            var isDirectory = item.Type == "tree";
            var segments = item.Path.Split('/');
            var nodeName = segments.Last();
            var parentPath = string.Join('/', segments.SkipLast(1));

            if (!directoryMap.ContainsKey(parentPath))
            {
                CreateParentDirectories(directoryMap, parentPath);
            }

            var node = new FileSystemNode
            {
                Name = nodeName,
                Path = item.Path,
                Url = item.Url,
                Sha = item.Sha,
                IsDirectory = isDirectory,
                Children = isDirectory ? [] : null,
                FileType = isDirectory ? FileType.Unknown : DetermineFileType(nodeName) // Set FileType
            };

            if (node.FileType == FileType.Code && nodeName.EndsWith(".cs99"))
            {
                node.Content = FetchFileContentAsync(item.Url).GetAwaiter().GetResult();
                rootNode.ClassInformationList.AddRange(ProcessCsFile(node, node.Content));
            }

            directoryMap[parentPath].Children.Add(node);

            if (isDirectory)
            {
                directoryMap[item.Path] = node;
            }
        }

        return rootNode;
    }

    private string CombineRoutes(string classRoute, string methodRoute)
    {
        // Ensure proper combination of class and method routes
        if (string.IsNullOrWhiteSpace(classRoute))
            return methodRoute;

        if (string.IsNullOrWhiteSpace(methodRoute))
            return classRoute;

        return $"{classRoute.TrimEnd('/')}/{methodRoute.TrimStart('/')}";
    }

    private HttpRequestResult<T> CreateGitHubRequest<T>(string requestPath)
    {
        var request = new HttpRequestResult<T>
        {
            CacheDurationMinutes = 300,
            RequestPath = requestPath
        };
        request.RequestHeaders.Add("User-Agent", "MarkHazletonWebSpark");
        request.RequestHeaders.Add("Authorization", $"token {_token}");
        return request;
    }
    private void CreateParentDirectories(Dictionary<string, FileSystemNode> directoryMap, string path)
    {
        if (string.IsNullOrEmpty(path) || directoryMap.ContainsKey(path))
        {
            return;
        }

        var segments = path.Split('/');
        var parentPath = string.Join('/', segments.SkipLast(1));
        var directoryName = segments.Last();

        // Recursively ensure the parent exists
        CreateParentDirectories(directoryMap, parentPath);

        // Create the current directory
        var directoryNode = new FileSystemNode
        {
            Name = directoryName,
            Path = path,
            IsDirectory = true,
            Children = []
        };

        directoryMap[parentPath].Children.Add(directoryNode);
        directoryMap[path] = directoryNode;
    }
    private FileType DetermineFileType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".txt" => FileType.Text,
            ".md" => FileType.Markdown,
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".svg" => FileType.Image,
            ".json" => FileType.Json,
            ".xml" => FileType.Xml,
            ".html" => FileType.Html,
            ".css" => FileType.Css,
            ".js" => FileType.Js,
            ".cs" or ".cpp" or ".py" or ".java" => FileType.Code,
            _ => FileType.Unknown,
        };
    }
    private List<MethodInfo> ExtractMethods(ClassDeclarationSyntax classDeclaration, string routePrefix, bool isApiClass)
    {
        return classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .Select(method =>
            {
                var methodRoute = GetRouteForMethod(method);
                var combinedRoute = CombineRoutes(routePrefix, methodRoute);
                var httpVerb = GetHttpVerb(method);
                var fullRoute = !string.IsNullOrWhiteSpace(combinedRoute) ? $"{httpVerb} {combinedRoute}" : null;
                var isEndPoint = isApiClass && !string.IsNullOrWhiteSpace(fullRoute);

                return new MethodInfo
                {
                    Name = method.Identifier.Text,
                    ReturnType = method.ReturnType.ToString(),
                    Parameters = method.ParameterList.Parameters
                        .Select(p => $"{p.Type} {p.Identifier.Text}")
                        .ToList(),
                    Comment = GetXmlComment(method),
                    IsPublic = method.Modifiers.Any(m => m.Text == "public"),
                    Route = fullRoute,
                    IsEndPoint = isEndPoint
                };
            })
            .ToList();
    }
    private List<PropertyInfo> ExtractProperties(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
            .Select(property => new PropertyInfo
            {
                Name = property.Identifier.Text,
                Type = property.Type.ToString(),
                Comment = GetXmlComment(property),
                IsPublic = property.Modifiers.Any(m => m.Text == "public")
            })
            .ToList();
    }
    private async Task<string> FetchFileContentAsync(string url, CancellationToken ct = default)
    {

        var request = CreateGitHubRequest<FileContent>(url);
        var response = await _httpRequestResultService.HttpSendRequestResultAsync<FileContent>(request);


        if (response.StatusCode == HttpStatusCode.OK)
        {
            return response.ResponseResults?.DecodedContent;
        }
        return string.Empty;
    }

    private async Task<FileSystemNode> GetFileSystemTreeAsync(string userName, string repoName, string branch, CancellationToken ct)
    {
        var endpoint = $"git/trees/{branch}?recursive=1";
        var treeResponse = await GetGitHubRepoDataAsync<GitHubTreeResponse>(userName, repoName, endpoint, ct);

        return BuildFileSystemTreeFromGitHub(treeResponse);
    }

    private async Task<T> GetGitHubRepoDataAsync<T>(string user, string repoName, string endpoint, CancellationToken ct)
    {
        var requestPath = string.IsNullOrEmpty(endpoint)
            ? $"https://api.github.com/repos/{user}/{repoName}"
            : $"https://api.github.com/repos/{user}/{repoName}/{endpoint}";

        var request = CreateGitHubRequest<T>(requestPath);
        var response = await _httpRequestResultService.HttpSendRequestResultAsync<T>(request);

        if (response.StatusCode == HttpStatusCode.OK && response.ResponseResults != null)
        {
            return response.ResponseResults;
        }
        else
        {
            var errors = string.Join(", ", response.ErrorList);
            throw new Exception($"Failed to fetch data: {errors}");
        }
    }

    private string GetHttpVerb(MethodDeclarationSyntax method)
    {
        // Detect the HTTP verb based on attributes
        var httpVerbAttribute = method.AttributeLists
            .SelectMany(a => a.Attributes)
            .FirstOrDefault(a => new[] { "HttpGet", "HttpPost", "HttpPut", "HttpDelete" }.Contains(a.Name.ToString()));

        return httpVerbAttribute?.Name.ToString().Replace("Http", string.Empty).ToUpperInvariant() ?? "GET";
    }

    private List<string> GetImplementedInterfaces(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.BaseList?.Types
            .Where(t => t.Type is IdentifierNameSyntax)
            .Select(t => t.Type.ToString())
            .ToList() ?? [];
    }
    private string GetInheritedClass(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.BaseList?.Types
            .Where(t => t.Type is SimpleNameSyntax)
            .Select(t => t.Type.ToString())
            .FirstOrDefault();
    }
    private string GetNamespace(SyntaxNode classNode)
    {
        var namespaceDeclaration = classNode.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        return namespaceDeclaration?.Name.ToString() ?? "Global";
    }
    private string GetRouteForMethod(MethodDeclarationSyntax method)
    {
        // Look for any route-related attributes
        var routeAttribute = method.AttributeLists
            .SelectMany(a => a.Attributes)
            .FirstOrDefault(a => a.Name.ToString().StartsWith("Http", StringComparison.OrdinalIgnoreCase));

        return routeAttribute?.ArgumentList?.Arguments.FirstOrDefault()?.ToString().Trim('\"');
    }
    private string GetRoutePrefix(ClassDeclarationSyntax classDeclaration, string className)
    {
        var routeAttribute = classDeclaration.AttributeLists
            .SelectMany(a => a.Attributes)
            .FirstOrDefault(a => a.Name.ToString().Contains("Route"));

        if (routeAttribute == null)
            return null;

        var routeArgument = routeAttribute.ArgumentList?.Arguments.FirstOrDefault()?.ToString().Trim('"');
        return AdjustRoutePrefix(routeArgument, className);
    }
    private string GetXmlComment(SyntaxNode node)
    {
        var trivia = node.GetLeadingTrivia()
            .Select(tr => tr.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        if (trivia == null)
            return null;

        var summaryNode = trivia.Content.OfType<XmlElementSyntax>()
            .FirstOrDefault(e => e.StartTag.Name.LocalName.Text == "summary");

        return summaryNode?.Content.ToFullString().Trim()
            .Replace("\n", " ")
            .Replace("\r", string.Empty)
            .Replace("///", string.Empty)
            .Replace("  ", " ");
    }
    private List<ClassInfo> ProcessCsFile(FileSystemNode fileNode, string fileContent)
    {
        var classes = new List<ClassInfo>();
        var syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
        var root = syntaxTree.GetCompilationUnitRoot();

        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

        foreach (var classDeclaration in classDeclarations)
        {
            var isApiClass = classDeclaration.AttributeLists
                .SelectMany(a => a.Attributes)
                .Any(a => a.Name.ToString().Contains("ApiController"));

            var routePrefix = isApiClass ? GetRoutePrefix(classDeclaration, classDeclaration.Identifier.Text) : null;

            var classInfo = new ClassInfo
            {
                Name = classDeclaration.Identifier.Text,
                Namespace = GetNamespace(classDeclaration),
                Comment = GetXmlComment(classDeclaration),
                InheritedClass = GetInheritedClass(classDeclaration),
                ImplementedInterfaces = GetImplementedInterfaces(classDeclaration),
                FilePath = fileNode.Path,
                RoutePrefix = routePrefix,
                Properties = ExtractProperties(classDeclaration),
                Methods = ExtractMethods(classDeclaration, routePrefix, isApiClass),
                IsApiClass = isApiClass
            };

            classes.Add(classInfo);
        }

        return classes;
    }

    public async Task<GitHubRepositoryAnalysisViewModel> AnalyzeRepositoryAsync(string userName, string repoName, CancellationToken ct)
    {
        var repoTask = GetGitHubRepoDataAsync<GitHubRepo>(userName, repoName, string.Empty, ct);
        var contributorsTask = GetGitHubRepoDataAsync<List<GitHubContributor>>(userName, repoName, "contributors", ct);
        var languagesTask = GetGitHubRepoDataAsync<Dictionary<string, int>>(userName, repoName, "languages", ct);
        var issuesTask = GetGitHubRepoDataAsync<List<GitHubIssue>>(userName, repoName, "issues", ct);

        var fileTreeTask = GetFileSystemTreeAsync(userName, repoName, "main", ct);

        await Task.WhenAll(repoTask, contributorsTask, languagesTask, issuesTask, fileTreeTask);

        return new GitHubRepositoryAnalysisViewModel
        {
            Repository = await repoTask,
            Contributors = await contributorsTask,
            Languages = await languagesTask,
            Issues = await issuesTask,
            FileSystemTree = await fileTreeTask,
            RepositoryName = repoName,
            UserName = userName
        };
    }


    public async Task<GitHubRepositoryAnalysisViewModel> GetRepositoryAnalysisAsync(string userName, string repoName, CancellationToken ct)
    {
        var repoList = _memoryCacheManager.Get<Dictionary<string, GitHubRepositoryAnalysisViewModel>>(_repoLookupListCacheKey, () => []);
        var key = $"{userName}/{repoName}";
        if (repoList.TryGetValue(key, out var repoAnalysis)) return repoAnalysis;
        if (repoList.Count >= MaxRepoLookupsPerHour && !repoList.ContainsKey(key))
        {
            throw new Exception("API lookup limit exceeded.");
        }
        var fetchedData = await AnalyzeRepositoryAsync(userName, repoName, ct);
        repoList[key] = fetchedData;
        _memoryCacheManager.Set(_repoLookupListCacheKey, repoList, 600);
        return fetchedData;
    }
}
