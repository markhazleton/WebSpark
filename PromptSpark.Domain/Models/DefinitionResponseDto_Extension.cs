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
        public static string JSONtoHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string json = input;

            // First, check if the input is valid JSON
            if (!IsValidJson(input))
            {
                // Look for JSON block marked with ```JSON and ending with ```
                int startIndex = input.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
                int endIndex = input.IndexOf("```", startIndex + 7, StringComparison.OrdinalIgnoreCase);

                // If both markers are found, extract the JSON block
                if (startIndex != -1 && endIndex != -1)
                {
                    // Extract JSON between ```json and ```
                    json = input.Substring(startIndex + 7, endIndex - (startIndex + 7)).Trim();

                    // Check if extracted text is valid JSON
                    if (!IsValidJson(json))
                    {
                        // If not valid JSON, return the original input unmodified
                        return input;
                    }
                }
                else
                {
                    // If no JSON block is found, return the original input
                    return input;
                }
            }

            try
            {
                // Pretty print the JSON if it's valid
                var prettyJson = JsonSerializer.Serialize(
                    JsonSerializer.Deserialize<object>(json),
                    new JsonSerializerOptions { WriteIndented = true });

                // Convert JSON to HTML with proper formatting
                return $"<pre class='language-json'><code class='language-json'>{System.Net.WebUtility.HtmlEncode(prettyJson)}</code></pre>";
            }
            catch (JsonException ex)
            {
                // Return the original input if any error occurs during formatting
                return input;
            }
        }

        // Helper method to check if a string is valid JSON
        private static bool IsValidJson(string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
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
