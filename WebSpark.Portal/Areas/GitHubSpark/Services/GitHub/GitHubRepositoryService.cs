using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Net;
using WebSpark.HttpClientUtility.MemoryCache;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;

/// <summary>
/// Service for interacting with GitHub repository APIs and analyzing repositories
/// </summary>
public class GitHubRepositoryService : IGitHubRepositoryService
{
    private readonly IHttpRequestResultService _httpRequestResultService;
    private readonly IMemoryCacheManager _memoryCacheManager;
    private readonly ILogger<GitHubRepositoryService> _logger;
    private readonly string _repoLookupListCacheKey = "repo-lookup-list";
    private readonly AsyncRetryPolicy<HttpRequestResult<object>> _retryPolicy;
    private readonly GitHubServiceOptions _options;

    /// <summary>
    /// Initializes a new instance of the GitHubRepositoryService
    /// </summary>
    public GitHubRepositoryService(
        IHttpRequestResultService httpClientSendService,
        IMemoryCacheManager memoryCacheManager,
        ILogger<GitHubRepositoryService> logger,
        IOptions<GitHubServiceOptions> options)
    {
        _httpRequestResultService = httpClientSendService;
        _memoryCacheManager = memoryCacheManager;
        _logger = logger;
        _options = options.Value;

        // Create a retry policy for transient HTTP errors
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .OrResult<HttpRequestResult<object>>(r =>
                r.StatusCode == HttpStatusCode.TooManyRequests ||
                r.StatusCode == HttpStatusCode.RequestTimeout ||
                (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                3, // Retry count
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning("Retrying GitHub API request (attempt {RetryAttempt}) after {RetryTimespan} due to: {ErrorMessage}",
                        retryAttempt, timespan, outcome.Exception?.Message ?? "HTTP error");
                });
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
        _logger.LogDebug("Building file system tree from GitHub tree response with SHA {Sha}", treeResponse.Sha);

        var rootNode = new FileSystemNode
        {
            Name = "root",
            Url = treeResponse.Url,
            Path = string.Empty,
            Sha = treeResponse.Sha,
            IsDirectory = true,
            Children = new List<FileSystemNode>()
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
                Children = isDirectory ? new List<FileSystemNode>() : null,
                FileType = isDirectory ? FileType.Unknown : DetermineFileType(nodeName) // Set FileType
            };

            if (node.FileType == FileType.Code && nodeName.EndsWith(".cs"))
            {
                try
                {
                    _logger.LogDebug("Processing C# file: {FilePath}", node.Path);
                    var content = FetchFileContentAsync(item.Url).GetAwaiter().GetResult();
                    node.Content = content;

                    if (!string.IsNullOrEmpty(content))
                    {
                        var classInfo = ProcessCsFile(node, content);
                        rootNode.ClassInformationList.AddRange(classInfo);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing C# file {FilePath}", node.Path);
                }
            }

            directoryMap[parentPath].Children.Add(node);

            if (isDirectory)
            {
                directoryMap[item.Path] = node;
            }
        }

        _logger.LogInformation("File system tree built with {FileCount} items", treeResponse.Tree.Count);
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
            CacheDurationMinutes = _options.DefaultCacheDurationMinutes,
            RequestPath = requestPath
        };
        request.RequestHeaders.Add("User-Agent", _options.UserAgent);
        request.RequestHeaders.Add("Authorization", $"token {_options.PersonalAccessToken}");
        request.RequestHeaders.Add("Accept", "application/vnd.github.v3+json");
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
            Children = new List<FileSystemNode>()
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
        try
        {
            _logger.LogDebug("Fetching file content from {Url}", url);
            var request = CreateGitHubRequest<FileContent>(url);
            var response = await _httpRequestResultService.HttpSendRequestResultAsync<FileContent>(request);

            if (response.StatusCode == HttpStatusCode.OK && response.ResponseResults != null)
            {
                return response.ResponseResults.DecodedContent;
            }

            _logger.LogWarning("Failed to fetch file content from {Url}. Status: {StatusCode}, Errors: {Errors}",
                url, response.StatusCode, string.Join(", ", response.ErrorList));
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching file content from {Url}", url);
            return string.Empty;
        }
    }

    private async Task<FileSystemNode> GetFileSystemTreeAsync(string userName, string repoName, string branch, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching file system tree for {UserName}/{RepoName}:{Branch}",
                userName, repoName, branch);

            var endpoint = $"git/trees/{branch}?recursive=1";
            var treeResponse = await GetGitHubRepoDataAsync<GitHubTreeResponse>(userName, repoName, endpoint, ct);

            return BuildFileSystemTreeFromGitHub(treeResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file system tree for {UserName}/{RepoName}:{Branch}",
                userName, repoName, branch);
            throw new GitHubApiException(
                $"Failed to get file system tree for {userName}/{repoName}:{branch}: {ex.Message}", ex);
        }
    }

    private async Task<T> GetGitHubRepoDataAsync<T>(string user, string repoName, string endpoint, CancellationToken ct)
    {
        var requestPath = string.IsNullOrEmpty(endpoint)
            ? $"https://api.github.com/repos/{user}/{repoName}"
            : $"https://api.github.com/repos/{user}/{repoName}/{endpoint}";

        try
        {
            _logger.LogDebug("Sending GitHub API request to {RequestPath}", requestPath);

            var request = CreateGitHubRequest<T>(requestPath);
            var response = await _httpRequestResultService.HttpSendRequestResultAsync<T>(request);

            if (response.StatusCode == HttpStatusCode.OK && response.ResponseResults != null)
            {
                return response.ResponseResults;
            }
            else
            {
                var errors = string.Join(", ", response.ErrorList);
                _logger.LogWarning("Failed API request to {RequestPath}. Status: {StatusCode}, Errors: {Errors}",
                    requestPath, response.StatusCode, errors);

                throw new GitHubApiException($"Failed to fetch data: {errors}")
                {
                    StatusCode = (int)response.StatusCode
                };
            }
        }
        catch (GitHubApiException)
        {
            throw; // Re-throw if already a GitHubApiException
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetGitHubRepoDataAsync for {RequestPath}", requestPath);
            throw new GitHubApiException($"Failed to fetch GitHub repository data: {ex.Message}", ex);
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
            .ToList() ?? new List<string>();
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

        try
        {
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
                    IsApiClass = isApiClass,
                    ProjectRoot = string.Empty
                };

                classes.Add(classInfo);
                _logger.LogDebug("Processed class {ClassName} in file {FilePath}", classInfo.Name, fileNode.Path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing C# file {FilePath}", fileNode.Path);
        }

        return classes;
    }

    /// <inheritdoc/>
    public async Task<GitHubRepositoryAnalysisViewModel> AnalyzeRepositoryAsync(string userName, string repoName, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Starting repository analysis for {UserName}/{RepoName}", userName, repoName);

            var repoTask = GetGitHubRepoDataAsync<GitHubRepo>(userName, repoName, string.Empty, ct);
            var contributorsTask = GetGitHubRepoDataAsync<List<GitHubContributor>>(userName, repoName, "contributors", ct);
            var languagesTask = GetGitHubRepoDataAsync<Dictionary<string, int>>(userName, repoName, "languages", ct);
            var issuesTask = GetGitHubRepoDataAsync<List<GitHubIssue>>(userName, repoName, "issues", ct);

            // Try both main and master branch names
            FileSystemNode fileSystemTree = null;
            try
            {
                fileSystemTree = await GetFileSystemTreeAsync(userName, repoName, "main", ct);
            }
            catch (GitHubApiException ex) when (ex.StatusCode == 404)
            {
                _logger.LogInformation("Main branch not found, trying master branch for {UserName}/{RepoName}",
                    userName, repoName);
                fileSystemTree = await GetFileSystemTreeAsync(userName, repoName, "master", ct);
            }

            await Task.WhenAll(repoTask, contributorsTask, languagesTask, issuesTask);

            var result = new GitHubRepositoryAnalysisViewModel
            {
                Repository = await repoTask,
                Contributors = await contributorsTask,
                Languages = await languagesTask,
                Issues = await issuesTask,
                FileSystemTree = fileSystemTree,
                RepositoryName = repoName,
                UserName = userName,
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation("Repository analysis completed for {UserName}/{RepoName}", userName, repoName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing repository {UserName}/{RepoName}", userName, repoName);
            throw new GitHubApiException($"Failed to analyze repository {userName}/{repoName}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<GitHubRepositoryAnalysisViewModel> GetRepositoryAnalysisAsync(string userName, string repoName, CancellationToken ct)
    {
        try
        {
            var cacheKey = $"{userName}/{repoName}";
            _logger.LogDebug("Checking cache for repository analysis {CacheKey}", cacheKey);

            // Check cache
            var repoList = _memoryCacheManager.Get<Dictionary<string, GitHubRepositoryAnalysisViewModel>>(
                _repoLookupListCacheKey,
                () => new Dictionary<string, GitHubRepositoryAnalysisViewModel>());

            // Return from cache if available
            if (repoList.TryGetValue(cacheKey, out var repoAnalysis))
            {
                _logger.LogDebug("Cache hit for repository analysis {CacheKey}", cacheKey);
                return repoAnalysis;
            }

            // Check rate limit
            if (repoList.Count >= _options.MaxRequestsPerHour && !repoList.ContainsKey(cacheKey))
            {
                _logger.LogWarning("API lookup limit exceeded for repository {CacheKey}", cacheKey);
                throw new GitHubApiException("API lookup limit exceeded. Try again later.")
                {
                    RateLimitInfo = new RateLimitInfo
                    {
                        Limit = _options.MaxRequestsPerHour,
                        Remaining = 0,
                        Reset = DateTime.UtcNow.AddHours(1)
                    }
                };
            }

            // Fetch and cache data
            _logger.LogInformation("Cache miss for repository analysis {CacheKey}, fetching from API", cacheKey);
            var fetchedData = await AnalyzeRepositoryAsync(userName, repoName, ct);

            repoList[cacheKey] = fetchedData;
            _memoryCacheManager.Set(_repoLookupListCacheKey, repoList, _options.DefaultCacheDurationMinutes * 2);

            return fetchedData;
        }
        catch (Exception ex) when (!(ex is GitHubApiException))
        {
            _logger.LogError(ex, "Error in GetRepositoryAnalysisAsync for {UserName}/{RepoName}", userName, repoName);
            throw new GitHubApiException($"Failed to retrieve repository analysis: {ex.Message}", ex);
        }
    }
}
