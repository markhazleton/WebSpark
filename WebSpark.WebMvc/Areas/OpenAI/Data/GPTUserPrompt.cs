using System.ComponentModel.DataAnnotations;

namespace PromptSpark.Areas.OpenAI.Data;

/// <summary>
/// Represents a response from the user in a PromptSpark chat.
/// </summary>
public class GPTUserPrompt
{
    [Key]
    public int Id { get; set; }
    public string UserPrompt { get; set; }
    public string DefinitionType { get; set; } = "Wichita";
    public string UserExpectedResponse { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Updated { get; set; } = DateTime.Now;
    public List<GPTDefinitionResponse> GPTResponses { get; set; } = new();
}
