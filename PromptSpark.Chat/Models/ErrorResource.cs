
namespace PromptSpark.Chat.Models;
/// <summary>
/// Error Resource
/// </summary>
public class ErrorResource
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="messages"></param>
    public ErrorResource(List<string> messages) { Messages = messages ?? []; }

    /// <summary>
    /// Error Resource
    /// </summary>
    /// <param name="message"></param>
    public ErrorResource(string message)
    {
        Messages = [];

        if (!string.IsNullOrWhiteSpace(message))
        {
            Messages.Add(message);
        }
    }

    /// <summary>
    /// Messages
    /// </summary>
    public List<string> Messages { get; private set; }

    /// <summary>
    /// Success
    /// </summary>
    public bool Success => false;
}
