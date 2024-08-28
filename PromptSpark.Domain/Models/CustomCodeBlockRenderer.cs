using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace PromptSpark.Domain.Models
{

    // Custom renderer for code blocks
    public class CustomCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
    {
        protected override void Write(HtmlRenderer renderer, CodeBlock obj)
        {
            // Determine the language class based on the fenced code block info
            var fencedCodeBlock = obj as FencedCodeBlock;
            string language = fencedCodeBlock?.Info ?? "plaintext";
            string languageClass = $"language-{language}";

            // Write the <pre> and <code> tags with the dynamic language class
            renderer.Write($"<pre class=\"{languageClass}\">")
                    .Write($"<code class=\"{languageClass}\">");

            renderer.WriteLeafRawLines(obj, true, true);

            renderer.Write("</code></pre>");
        }

    }
}