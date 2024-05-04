using PromptSpark.Areas.OpenAI.Models;
using System.ComponentModel.DataAnnotations;

namespace PromptSpark.Areas.OpenAI.Data;

/// <summary>
/// Represents a system response in a PromptSpark chat.
/// </summary>
public class GPTDefinitionResponse
{
    public GPTDefinitionResponse()
    {

    }
    public GPTDefinitionResponse(GPTDefinition definition, GPTUserPrompt userPrompt)
    {
        DefinitionId = definition.DefinitionId;
        Definition = definition;
        ResponseId = userPrompt.Id;
        Response = userPrompt;
        GPTName = definition.GPTName;
        GPTDescription = definition.Description;
        OutputType = definition.OutputType;
        Temperature = definition.Temperature;
        DefinitionType = definition.DefinitionType;
        Model = definition.Model;
        SystemPrompt = definition.Prompt;
        UserPrompt = userPrompt.UserPrompt;
        UserExpectedResponse = userPrompt?.UserExpectedResponse ?? "UNKNOWN";
        Created = DateTime.Now;
        Updated = DateTime.Now;
    }



    public int CompletionTokens { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public GPTDefinition Definition { get; set; }
    public int DefinitionId { get; set; }
    public string DefinitionType { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public string GPTName { get; set; }
    [Key]
    public int Id { get; set; }
    public string Model { get; set; }
    public int PromptTokens { get; set; }
    public GPTUserPrompt Response { get; set; }
    public int ResponseId { get; set; }
    public string? SystemPrompt { get; set; }
    public string SystemResponse { get; set; }
    public string Temperature { get; set; }
    public int TotalTokens { get; set; }
    public DateTime Updated { get; set; } = DateTime.Now;
    public string? UserPrompt { get; set; }
    public OutputType OutputType { get; set; }
    public string GPTDescription { get; set; }
    public string UserExpectedResponse { get; set; }

}
