using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PromptSpark.Domain.Models
{
    public static class DefinitionResponseDto_Extension
    {
        public static string ContentToString(this DefinitionResponseDto response)
        {
            if (string.IsNullOrEmpty(response.SystemResponse))
                return string.Empty;
            // switch on output type
            return response.OutputType switch
            {
                OutputType.Markdown => MarkdownToHtml(response.SystemResponse),
                OutputType.DevOpsMarkdown => MarkdownEncodeHtml(response.SystemResponse),
                OutputType.PUG => PugToHtml(response.SystemResponse),
                OutputType.JSON => JSONtoHtml(response.SystemResponse),
                _ => response.SystemResponse,
            };
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
                return $"<pre class='language-json'><code class='language-json'>{System.Net.WebUtility.HtmlEncode(prettyJson)}</code></pre>";
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing errors (optional: log the error)
                return $"<p>Error parsing JSON: {System.Net.WebUtility.HtmlEncode(ex.Message)}</p>";
            }
        }
        public static string PugToHtml(string input)
        {

            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Define the regex pattern
            string pattern = @"```pug\s*([\s\S]*?)\s*```";

            // Perform the regex match
            Match match = Regex.Match(input, pattern);

            // If a match is found, return the captured group; otherwise, return an empty string
            if (match.Success)
            {
                return $"<pre class='language-pug'><code class='language-pug'>{match.Groups[1].Value.Trim()}</code></pre>";
            }

            // Define the start and end markers
            string startMarker = "```pug";
            string endMarker = "```";

            // Find the start index of the pug section
            int startIndex = input.IndexOf(startMarker);

            // If the start marker is not found, return an empty string
            if (startIndex == -1)
            {
                return input;
            }

            // Adjust start index to skip the start marker
            startIndex += startMarker.Length;

            // Find the end index of the pug section
            int endIndex = input.IndexOf(endMarker, startIndex);

            // If the end marker is not found, return an empty string
            if (endIndex == -1)
            {
                return input;
            }

            // Extract the text between the start and end markers
            string extractedText = input[startIndex..endIndex].Trim();

            return $"<pre class='language-pug'><code class='language-pug'>{extractedText}</code></pre>";
        }
        public static string MarkdownEncodeHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return string.Empty;

            markdown = markdown.Replace("<", "&lt;");
            return $"<pre class='language-markdown'><code class='language-markdown'>{markdown}</code></pre>";
        }
        public static string MarkdownToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return string.Empty;

            // Create a custom pipeline
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            // Create a custom HTML renderer
            var writer = new StringWriter();
            var renderer = new HtmlRenderer(writer);

            // Customize the code block rendering
            renderer.ObjectRenderers.Replace<CodeBlockRenderer>(new CustomCodeBlockRenderer());

            // Render the markdown
            var document = Markdown.Parse(markdown, pipeline);
            renderer.Render(document);
            writer.Flush();

            return writer.ToString();
        }
    }
}
