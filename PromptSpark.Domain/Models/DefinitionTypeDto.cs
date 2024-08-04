namespace PromptSpark.Domain.Models;

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
    PUG,
    Other,
    DevOpsMarkdown
}

public class DefinitionTypeDto
{
    public string DefinitionType { get; set; }
    public string? Description { get; set; }
    public OutputType OutputType { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Updated { get; set; } = DateTime.Now;
    public List<DefinitionDto> Definitions { get; set; } = [];
    public List<UserPromptDto> Prompts { get; set; } = [];
    public int CurrentUserPromptId { get; set; }
}
