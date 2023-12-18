using Markdig;

namespace ControlSpark.Core.Extensions;

public static class StringExtensions
{
    public static string ToHtmlFromMarkdown(this string markdown)
    {
        return Markdown.ToHtml(markdown);
    }
}
