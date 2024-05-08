namespace WebSpark.Prompt.Models;

public enum GptRole
{
    system,
    user
}

public enum OutputType
{
    Markdown,
    JSON,
    Text,
    HTML,
    Other
}

public class DefinitionTypeDto
{
    public string DefinitionType { get; set; }
    public string? Description { get; set; }
    public OutputType OutputType { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;
    public List<DefinitionDto> Definitions { get; set; } = [];
    public List<UserPromptDto> Prompts { get; set; } = [];
}
