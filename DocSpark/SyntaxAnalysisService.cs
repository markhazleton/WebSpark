using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DocSpark;

/// <summary>
/// Provides functionality to analyze syntax of C# source files and extract class details.
/// </summary>
public class SyntaxAnalysisService
{
    private readonly ILogger<SyntaxAnalysisService> _logger;
    private readonly HashSet<string> _ignoredDirectories;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxAnalysisService"/> class.
    /// </summary>
    /// <param name="logger">An instance of <see cref="ILogger{SyntaxAnalysisService}"/> for logging purposes.</param>
    public SyntaxAnalysisService(ILogger<SyntaxAnalysisService> logger)
    {
        _logger = logger;
        _ignoredDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj", "node_modules" };
    }

    /// <summary>
    /// Extracts class information from the provided file system node, recursively analyzing all .cs files.
    /// </summary>
    /// <param name="node">The root file system node to analyze.</param>
    /// <param name="projectRoot">The root directory of the project for reference.</param>
    /// <returns>A list of <see cref="ClassInfo"/> objects representing the extracted class information.</returns>
    public List<ClassInfo> ExtractClassesFromFileSystemNode(FileSystemNode node, string projectRoot)
    {
        if (node == null)
        {
            _logger.LogError("FileSystemNode is null. Cannot proceed with syntax analysis.");
            return new List<ClassInfo>();
        }

        var classes = new ConcurrentBag<ClassInfo>();

        ProcessNodeRecursively(node, projectRoot, classes);

        return classes.ToList();
    }

    private void ProcessNodeRecursively(FileSystemNode node, string projectRoot, ConcurrentBag<ClassInfo> classes)
    {
        if (node.IsDirectory)
        {
            if (_ignoredDirectories.Contains(Path.GetFileName(node.Path)))
            {
                _logger.LogInformation("Skipping ignored directory: {DirectoryPath}", node.Path);
                return;
            }

            if (node.Children != null)
            {
                Parallel.ForEach(node.Children, child => ProcessNodeRecursively(child, projectRoot, classes));
            }
        }
        else if (node.Path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var classInfos = ProcessCsFile(node, projectRoot);
                foreach (var classInfo in classInfos)
                {
                    classes.Add(classInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FilePath}", node.Path);
            }
        }
        else
        {
            _logger.LogDebug("Skipping non-C# file: {FilePath}", node.Path);
        }
    }

    private List<ClassInfo> ProcessCsFile(FileSystemNode fileNode, string projectRoot)
    {
        var classes = new List<ClassInfo>();
        string fileContent = File.ReadAllText(fileNode.Path);
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
                ProjectRoot = projectRoot,
                RoutePrefix = routePrefix,
                Properties = ExtractProperties(classDeclaration),
                Methods = ExtractMethods(classDeclaration, routePrefix, isApiClass),
                IsApiClass = isApiClass
            };

            classes.Add(classInfo);
        }

        return classes;
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

    private string AdjustRoutePrefix(string routePrefix, string className)
    {
        if (string.IsNullOrWhiteSpace(routePrefix) || !routePrefix.Contains("[controller]", StringComparison.OrdinalIgnoreCase))
            return routePrefix;

        var controllerBaseName = className.EndsWith("Controller")
            ? className.Substring(0, className.Length - "Controller".Length).ToLowerInvariant()
            : className.ToLowerInvariant();

        return routePrefix.Replace("[controller]", controllerBaseName, StringComparison.OrdinalIgnoreCase);
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

    private string GetRouteForMethod(MethodDeclarationSyntax method)
    {
        // Look for any route-related attributes
        var routeAttribute = method.AttributeLists
            .SelectMany(a => a.Attributes)
            .FirstOrDefault(a => a.Name.ToString().StartsWith("Http", StringComparison.OrdinalIgnoreCase));

        return routeAttribute?.ArgumentList?.Arguments.FirstOrDefault()?.ToString().Trim('\"');
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

    private string GetHttpVerb(MethodDeclarationSyntax method)
    {
        // Detect the HTTP verb based on attributes
        var httpVerbAttribute = method.AttributeLists
            .SelectMany(a => a.Attributes)
            .FirstOrDefault(a => new[] { "HttpGet", "HttpPost", "HttpPut", "HttpDelete" }.Contains(a.Name.ToString()));

        return httpVerbAttribute?.Name.ToString().Replace("Http", string.Empty).ToUpperInvariant() ?? "GET";
    }


    private string GetNamespace(SyntaxNode classNode)
    {
        var namespaceDeclaration = classNode.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        return namespaceDeclaration?.Name.ToString() ?? "Global";
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

    private string GetInheritedClass(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.BaseList?.Types
            .Where(t => t.Type is SimpleNameSyntax)
            .Select(t => t.Type.ToString())
            .FirstOrDefault();
    }

    private List<string> GetImplementedInterfaces(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.BaseList?.Types
            .Where(t => t.Type is IdentifierNameSyntax)
            .Select(t => t.Type.ToString())
            .ToList() ?? new List<string>();
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
}
