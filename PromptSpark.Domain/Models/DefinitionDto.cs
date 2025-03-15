using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PromptSpark.Domain.Models;

public class DefinitionDto
{
    public int DefinitionId { get; set; }
    public string Description { get; set; } = "Description";
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Updated { get; set; } = DateTime.Now;
    public string Name { get; set; } = "Name";
    public string UrlEncodedName { get; set; } = "Name";
    public OutputType OutputType { get; set; }
    public string Prompt { get; set; } = "System Prompt";
    public string PromptHash { get; set; } = string.Empty;
    public string DefinitionType { get; set; } = "Wichita";
    public List<string> DefinitionTypes { get; set; } = ["Wichita"];
    public GptRole Role { get; set; } = GptRole.system;
    public string Model { get; set; } = "gpt-3.5-turbo";
    public string Temperature { get; set; } = "0.7";
    public List<DefinitionResponseDto> DefinitionResponses { get; set; } = [];
    public Guid ConversationId { get; set; } = Guid.NewGuid();
    public string Slug
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return "UNKNOWN";
            }

            // Normalize the string
            string normalized = Name.Normalize(NormalizationForm.FormD);

            // Remove diacritic marks (accents)
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            string cleaned = sb.ToString().Normalize(NormalizationForm.FormC);

            // Convert to lowercase
            cleaned = cleaned.ToLowerInvariant();

            // Replace spaces with dashes
            cleaned = Regex.Replace(cleaned, @"\s+", "-");

            // Remove invalid URL characters
            cleaned = Regex.Replace(cleaned, @"[^a-z0-9\-]", string.Empty);

            // Trim dashes from start and end
            cleaned = cleaned.Trim('-');

            if (string.IsNullOrEmpty(cleaned))
            {
                cleaned = "unknown";
            }
            return cleaned;
        }
    }
}
