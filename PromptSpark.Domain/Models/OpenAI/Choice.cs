using System.Text.Json.Serialization;

namespace PromptSpark.Domain.Models.OpenAI;

public class Choice
{
    public Dictionary<string, object> AdditionalProperties { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; }

    [JsonPropertyName("logprobs")]
    public object Logprobs { get; set; } // Assuming this can be null

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }

}
