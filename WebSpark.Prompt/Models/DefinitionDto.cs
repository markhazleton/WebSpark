namespace WebSpark.Prompt.Models;

public class DefinitionDto
{
    public int DefinitionId { get; set; }
    public string Description { get; set; } = "Description";
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;
    public string Name { get; set; } = "Name";
    public OutputType OutputType { get; set; }
    public string Prompt { get; set; } = "System Prompt";
    public string PromptHash { get; set; } = string.Empty;
    public string DefinitionType { get; set; } = "Wichita";
    public List<string> DefinitionTypes { get; set; } = ["Wichita"];
    public GptRole Role { get; set; } = GptRole.system;
    public string Model { get; set; } = "gpt-3.5-turbo";
    public string Temperature { get; set; } = "0.7";
    public List<DefinitionResponseDto> DefinitionResponses { get; set; } = [];
}
