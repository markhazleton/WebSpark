namespace PromptSpark.Areas.OpenAI.Models.OpenAI;

using System.Collections.Generic;

public class ChatCompletionsResponse
{
    /// <summary>
    /// A unique identifier for the chat completion.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The object type, which is always 'chat.completion'.
    /// </summary>
    public string Object { get; set; }

    /// <summary>
    /// The Unix timestamp (in seconds) of when the chat completion was created.
    /// </summary>
    public int Created { get; set; }

    /// <summary>
    /// The model used for the chat completion.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// A list of chat completion choices. Can be more than one if 'n' is greater than 1.
    /// Each choice represents a potential message response from the model.
    /// </summary>
    public List<Choice> Choices { get; set; }

    /// <summary>
    /// usage statistics for the completion request. Includes token counts.
    /// </summary>
    public Usage Usage { get; set; }

    /// <summary>
    /// This fingerprint represents the backend configuration that the model runs with.
    /// It can be used in conjunction with the seed request parameter to understand when backend changes have been made that might impact determinism.
    /// </summary>
    public string SystemFingerprint { get; set; }
}



