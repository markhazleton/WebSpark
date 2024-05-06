using System.ComponentModel.DataAnnotations;
using WebSpark.Prompt.Models;

namespace WebSpark.Prompt.Data;

/// <summary>
/// Represents the definition of a PromptSpark chat.
/// </summary>
public class GPTDefinition
{
    [Key]
    public int DefinitionId { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Updated { get; set; } = DateTime.Now;
    public string GPTName { get; set; } = "Name";
    public string Description { get; set; } = "Description";
    public OutputType OutputType { get; set; }
    public string Prompt { get; set; } = "System Prompt";
    public string PromptHash { get; set; } = string.Empty;
    public string DefinitionType { get; set; } = "Wichita";
    public GptRole Role { get; set; } = GptRole.system;
    public string Model { get; set; } = "gpt-3.5-turbo";
    public string Temperature { get; set; } = "0.7";
    public List<GPTDefinitionResponse> GPTResponses { get; set; } = [];
}
