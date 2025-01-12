using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DocSpark;

/// <summary>
/// Generates Markdown documentation from a list of classes.
/// </summary>
public class MarkdownGenerator
{
    private readonly ILogger<MarkdownGenerator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownGenerator"/> class.
    /// </summary>
    /// <param name="logger">An instance of <see cref="ILogger{MarkdownGenerator}"/> for logging purposes.</param>
    public MarkdownGenerator(ILogger<MarkdownGenerator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates Markdown documentation for the provided classes.
    /// </summary>
    /// <param name="classes">The list of classes to document.</param>
    /// <returns>A string containing the generated Markdown.</returns>
    public string GenerateMarkdown(List<ClassInfo> classes)
    {
        if (classes == null || !classes.Any())
        {
            _logger.LogWarning("No classes provided for Markdown generation.");
            return "# Project Documentation\n\nNo classes found to document.";
        }

        var markdownBuilder = new StringBuilder();
        markdownBuilder.AppendLine("# Project Documentation");
        markdownBuilder.AppendLine();

        foreach (var classInfo in classes)
        {
            try
            {
                AppendClassDocumentation(markdownBuilder, classInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Markdown for class: {ClassName}", classInfo?.Name);
            }
        }

        return markdownBuilder.ToString();
    }

    /// <summary>
    /// Appends the documentation for a class to the Markdown builder.
    /// </summary>
    private void AppendClassDocumentation(StringBuilder markdownBuilder, ClassInfo classInfo)
    {
        if (classInfo == null)
        {
            _logger.LogWarning("Encountered a null ClassInfo object. Skipping.");
            return;
        }

        var classLink = $"[{classInfo.Name}]({classInfo.RelativePath()})";

        markdownBuilder.AppendLine($"## Class: {classLink}");
        markdownBuilder.AppendLine($"Namespace: `{classInfo.Namespace}`");

        if (!string.IsNullOrWhiteSpace(classInfo.Comment))
        {
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine($"**{classInfo.Comment.Trim()}**");
        }

        if (!string.IsNullOrWhiteSpace(classInfo.InheritedClass))
        {
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine($"- Inherits: `{classInfo.InheritedClass}`");
        }

        if (classInfo.ImplementedInterfaces?.Count > 0)
        {
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine($"- Implements: {string.Join(", ", classInfo.ImplementedInterfaces.Select(i => $"`{i}`"))}");
        }

        if (!string.IsNullOrWhiteSpace(classInfo.RoutePrefix))
        {
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine($"- Route Prefix: `{classInfo.RoutePrefix}`");
        }

        markdownBuilder.AppendLine();
        AppendProperties(markdownBuilder, classInfo.Properties);
        AppendMethods(markdownBuilder, classInfo.Methods, classInfo.RoutePrefix, classInfo.Name);
        markdownBuilder.AppendLine("---");
    }

    private void AppendProperties(StringBuilder markdownBuilder, List<PropertyInfo> properties)
    {
        if (properties == null || !properties.Any())
        {
            _logger.LogInformation("No properties found to document.");
            return;
        }

        var publicProps = properties.Where(p => p.IsPublic).ToList();
        var privateProps = properties.Where(p => !p.IsPublic).ToList();

        if (publicProps.Any())
        {
            markdownBuilder.AppendLine("### Public Properties:");
            foreach (var property in publicProps)
            {
                AppendProperty(markdownBuilder, property);
            }
            markdownBuilder.AppendLine();
        }

        if (privateProps.Any())
        {
            markdownBuilder.AppendLine("### Private Properties:");
            foreach (var property in privateProps)
            {
                AppendProperty(markdownBuilder, property);
            }
            markdownBuilder.AppendLine();
        }
    }

    private void AppendProperty(StringBuilder markdownBuilder, PropertyInfo property)
    {
        markdownBuilder.AppendLine($"- `{property.Name}`: `{property.Type}` **{property.Comment?.Trim()}**");
    }

    private void AppendMethods(StringBuilder markdownBuilder, List<MethodInfo> methods, string routePrefix, string controllerName)
    {
        if (methods == null || !methods.Any())
        {
            _logger.LogInformation("No methods found to document.");
            return;
        }

        var publicMethods = methods.Where(m => m.IsPublic).ToList();
        var privateMethods = methods.Where(m => !m.IsPublic).ToList();

        if (publicMethods.Any())
        {
            markdownBuilder.AppendLine("### Public Methods:");
            foreach (var method in publicMethods)
            {
                AppendMethod(markdownBuilder, method, routePrefix, controllerName);
            }
            markdownBuilder.AppendLine();
        }

        if (privateMethods.Any())
        {
            markdownBuilder.AppendLine("### Private Methods:");
            foreach (var method in privateMethods)
            {
                AppendMethod(markdownBuilder, method, routePrefix, controllerName);
            }
            markdownBuilder.AppendLine();
        }
    }

    private void AppendMethod(StringBuilder markdownBuilder, MethodInfo method, string routePrefix, string controllerName)
    {
        var parameters = string.Join(", ", method.Parameters);
        var routeWithVerb = $"{method.Route}".TrimEnd('/');

        markdownBuilder.AppendLine($"- `{method.Name}({parameters})`: Returns `{method.ReturnType}` **{method.Comment?.Trim()}**");

        if (!string.IsNullOrWhiteSpace(method.Route))
        {
            markdownBuilder.AppendLine($"  - Route: `{routeWithVerb}`");
        }
    }
}
