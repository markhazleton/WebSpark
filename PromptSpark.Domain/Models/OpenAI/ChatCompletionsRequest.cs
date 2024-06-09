namespace PromptSpark.Domain.Models.OpenAI;

using System.Collections.Generic;
public class ChatCompletionsRequest
{
    /// <summary>
    /// Required. ID of the model to use. Check the model endpoint compatibility table for details on which models work with the Chat API.
    /// </summary>
    public string model { get; set; }

    /// <summary>
    /// Required. A list of messages comprising the conversation so far.
    /// </summary>
    public List<Message> messages { get; set; }

    /// <summary>
    /// Optional. Specifies the sampling temperature to use. The range is from 0 to 2. Lower values make the output more deterministic.
    /// </summary>
    public double? temperature { get; set; }

    /// <summary>
    /// Optional. A number between -2.0 and 2.0 that penalizes new tokens based on their frequency in the text so far. Defaults to 0.
    /// </summary>
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    /// Optional. Modifies the likelihood of specified tokens appearing in the completion. Maps tokens to bias values from -100 to 100.
    /// </summary>
    public Dictionary<int, double> LogitBias { get; set; }

    /// <summary>
    /// Optional. Whether to return log probabilities of the output tokens. Defaults to false.
    /// </summary>
    public bool? Logprobs { get; set; }

    /// <summary>
    /// Optional. Specifies the number of most likely tokens to return at each token position. Requires Logprobs to be true.
    /// </summary>
    public int? TopLogprobs { get; set; }

    /// <summary>
    /// Optional. The maximum number of tokens that can be generated in the chat completion.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Optional. The number of chat completion choices to generate for each input message. Defaults to 1.
    /// </summary>
    public int? N { get; set; }

    /// <summary>
    /// Optional. A number between -2.0 and 2.0 that penalizes new tokens based on their appearance in the text so far. Defaults to 0.
    /// </summary>
    public double? PresencePenalty { get; set; }

    /// <summary>
    /// Optional. Specifies the format that the model must output, useful for structured data like JSON.
    /// </summary>
    public ResponseFormat response_format { get; set; }

    /// <summary>
    /// Optional. If specified, aims for deterministic results using the same seed and parameters. Determinism is not guaranteed.
    /// </summary>
    public int? Seed { get; set; }

    /// <summary>
    /// Optional. Sequences where the API will stop generating further tokens.
    /// </summary>
    public List<string> Stop { get; set; }

    /// <summary>
    /// Optional. If set to true, partial message deltas will be sent as they become available.
    /// </summary>
    public bool? Stream { get; set; }

    /// <summary>
    /// Optional. An alternative to sampling with temperature, where only the tokens comprising the top_p probability mass are considered.
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    /// Optional. A list of tools the model may call.
    /// </summary>
    public List<string> Tools { get; set; }

    /// <summary>
    /// Optional. Controls which function, if any, is called by the model.
    /// </summary>
    public ToolChoice ToolChoice { get; set; }

    /// <summary>
    /// Optional. A unique identifier for the end-user, helping to monitor and detect abuse.
    /// </summary>
    public string User { get; set; }
}

public class ResponseFormat
{
    public string type { get; set; }
}

public class ToolChoice
{
    public string Type { get; set; }
    public Function Function { get; set; }
}

public class Function
{
    public string Name { get; set; }
}
