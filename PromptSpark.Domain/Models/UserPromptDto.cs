namespace PromptSpark.Domain.Models;

public class UserPromptDto
{
    public int UserPromptId { get; set; }
    public string UserPrompt { get; set; }
    public string DefinitionType { get; set; }
    public string UserExpectedResponse { get; set; }
    public List<string> DefinitionTypes { get; set; } = ["Wichita"];
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public List<DefinitionResponseDto> DefinitionResponses { get; set; } = [];
}
