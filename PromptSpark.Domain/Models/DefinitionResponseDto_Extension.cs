using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace PromptSpark.Domain.Models;

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


        // Check for JSON code block markers like ```json ... ```
        if (!IsValidJson(input))
        {
            int startIndex = input.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
            int endIndex = input.IndexOf("```", startIndex + 7, StringComparison.OrdinalIgnoreCase);

            if (startIndex != -1 && endIndex != -1)
            {
                // Extract JSON text between ```json and ```
                json = input.Substring(startIndex + 7, endIndex - (startIndex + 7)).Trim();
            }
            else
            {
                string cleanedJson = CleanJsonString(json);
                try
                {
                    var jop = JObject.Parse(cleanedJson);


                }
                catch (Exception ex)
                {

                }

                if (IsValidJson(cleanedJson))
                {
                    json = cleanedJson;
                }
                // Check if cleaned input is valid JSON; return original if not
                if (!IsValidJson(json))
                    json = ConvertToValidJsonUsingNewtonsoft(input);

                // Check if cleaned input is valid JSON; return original if not
                if (!IsValidJson(json))
                    return input;
            }
        }

        try
        {
            // Parse JSON to ensure it's valid and reformat it with indentation
            var parsedJson = JToken.Parse(json);
            string prettyJson = parsedJson.ToString(Formatting.Indented);

            // Convert formatted JSON to HTML with proper encoding
            return $"<pre class='language-json'><code class='language-json'>{WebUtility.HtmlEncode(prettyJson)}</code></pre>";
        }
        catch (JsonReaderException)
        {
            // If parsing fails, return the original input
            return input;
        }
    }

    // Checks if the input string is valid JSON
    public static bool IsValidJson(string input)
    {
        try
        {
            JToken.Parse(input);


            return true;
        }
        catch (JsonReaderException)
        {
            return false;
        }
    }

    // Attempts to convert potentially malformed JSON using Newtonsoft.Json
    public static string ConvertToValidJsonUsingNewtonsoft(string input)
    {
        // Step 1: Replace \r\n with \\n for JSON compliance
        string cleanedInput = input.Replace("\\r\\n", "\\n")   // Convert Windows line breaks to JSON-compliant newlines
                                   .Replace("\\n", "\\n")      // Ensure all newlines are double-escaped
                                   .Replace("\\\"", "\"")      // Replace escaped quotes with standard quotes
                                   .Replace("\\\\", "\\");     // Ensure backslashes are consistent

        // Step 2: Parse the cleaned JSON string
        try
        {
            var jsonObject = JObject.Parse(cleanedInput);
            return jsonObject.ToString(Formatting.Indented); // Format for readability
        }
        catch (JsonReaderException)
        {
            // If parsing fails, return the original input for further inspection
            return input;
        }
    }

    public static string CleanJsonString(string input)
    {
        // Step 1: Replace `\\r\\n` and `\\n` with actual newlines, or remove them if they are not needed
        string cleanedInput = input.Replace("\\r\\n", string.Empty)
                                   .Replace("\\n", string.Empty);

        // Step 2: Remove unnecessary backslashes before quotes
        cleanedInput = Regex.Replace(cleanedInput, @"\\(?=[""{}:\[\],])", string.Empty);

        // Step 3: Convert double backslashes `\\` to single backslashes `\` if any exist for path formatting etc.
        cleanedInput = cleanedInput.Replace("\\\\", "\\");

        return cleanedInput;
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
            .UseGenericAttributes()
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
