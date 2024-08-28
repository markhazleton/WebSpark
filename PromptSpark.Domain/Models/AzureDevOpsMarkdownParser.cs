using Markdig;
using Markdig.Extensions.AutoIdentifiers;

namespace PromptSpark.Domain.Models;

public class AzureDevOpsMarkdownParser
{
    private readonly MarkdownPipeline _pipeline;

    public AzureDevOpsMarkdownParser()
    {
        // Configure the Markdig pipeline to mimic Azure DevOps Wiki's Markdown rules
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()                        // Enables a suite of extensions
            .UsePipeTables()                                // Pipe tables support
            .UseEmphasisExtras()                            // Extra emphasis like underscores
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub) // GitHub-style header links
            .UseListExtras()                                // Additional list syntax options
            .UseTaskLists()                                 // Support for task lists
            .UseAutoLinks()                                 // Automatically link URLs
            .DisableHtml()                                  // Optional: disable raw HTML for security
            .Build();
    }

    public string AzureDevOpsWikiToHtml(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            throw new ArgumentException("Markdown content cannot be null or empty.", nameof(markdown));

        // Parse the Markdown string to HTML using the customized pipeline
        return Markdown.ToHtml(markdown, _pipeline);
    }
}
