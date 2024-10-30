using System.Text.Json.Serialization;

namespace PromptSpark.Chat.PromptFlow;

public class ChatEntry
{
    public DateTime Timestamp { get; set; }
    public string User { get; set; }
    public string UserMessage { get; set; }
    public string BotResponse { get; set; }
}

public class Workflow
{
    [JsonPropertyName("workflowId")]
    public string WorkflowId { get; set; }

    [JsonPropertyName("startNode")]
    public string StartNode { get; set; }

    [JsonPropertyName("nodes")]
    public List<Node> Nodes { get; set; }
}

public class Node
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("question")]
    public string Question { get; set; }

    [JsonPropertyName("answers")]
    public List<Answer> Answers { get; set; }
}

public class Answer
{
    [JsonPropertyName("response")]
    public string Response { get; set; }

    [JsonPropertyName("nextNode")]
    public string NextNode { get; set; }
}

public class WorkflowNodeResponse
{
    [JsonPropertyName("question")]
    public string Question { get; set; }

    [JsonPropertyName("answers")]
    public List<AnswerOption> Answers { get; set; }
}

public class AnswerOption
{
    [JsonPropertyName("response")]
    public string Response { get; set; }

    [JsonPropertyName("link")]
    public string Link { get; set; }
}