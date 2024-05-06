using Markdig;
using System.Text.Json;

namespace WebSpark.Prompt.Models;

public static class DefinitionResponseDto_Extension
{
    public static string MarkdownToHtml(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return string.Empty;

        return Markdown.ToHtml(markdown); // Convert markdown text to HTML
    }

    public static string JSONtoHtml(string json)
    {
        if (string.IsNullOrEmpty(json))
            return string.Empty;

        try
        {
            // Attempt to parse the JSON string to validate it
            var jsonObject = JsonDocument.Parse(json);

            // If JSON is valid, pretty print it using JsonSerializer
            var prettyJson = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<object>(json),
                new JsonSerializerOptions { WriteIndented = true });

            // Convert JSON text to HTML with proper formatting
            return $"<pre>{System.Net.WebUtility.HtmlEncode(prettyJson)}</pre>";
        }
        catch (JsonException ex)
        {
            // Handle JSON parsing errors (optional: log the error)
            return $"<p>Error parsing JSON: {System.Net.WebUtility.HtmlEncode(ex.Message)}</p>";
        }
    }

    public static string ContentToString(this DefinitionResponseDto response)
    {
        if (string.IsNullOrEmpty(response.SystemResponse))
            return string.Empty;
        // switch on output type
        return response.OutputType switch
        {
            OutputType.Markdown => MarkdownToHtml(response.SystemResponse),
            OutputType.HTML => response.SystemResponse,
            OutputType.JSON => JSONtoHtml(response.SystemResponse),
            _ => response.SystemResponse,
        };
    }
}

