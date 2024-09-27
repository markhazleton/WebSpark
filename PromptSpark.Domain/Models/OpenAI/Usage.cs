using System.Text.Json.Serialization;

namespace PromptSpark.Domain.Models.OpenAI;

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("completion_tokens_details")]
    public CompletionTokensDetails CompletionTokensDetails { get; set; }
}

public class CompletionTokensDetails
{
    [JsonPropertyName("reasoning_tokens")]
    public int ReasoningTokens { get; set; }
}