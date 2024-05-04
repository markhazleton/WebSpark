namespace PromptSpark.Areas.OpenAI.Models;
public class DefinitionResponseDto
{
    public string GPTName { get; set; }
    public string SystemResponse { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public long TimeMS { get; set; }
    public int TotalTokens { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public string SystemPrompt { get; set; }
    public string MessageType { get; set; } = "Wichita";
    public string Role { get; set; } = "system";
    public string Temperature { get; set; }
    public string Model { get; set; }
    public string UserPrompt { get; set; }
    public OutputType OutputType { get; set; }
    public string GPTDescription { get; set; }
    public string UserExpectedResponse { get; set; }
    public override string ToString()
    {
        return $"<dl> <dt>Open AI Model</dt><dd>{Model}</dd><dt>System Prompt</dt><dd>{SystemPrompt}</dd></dl>";
    }
}
