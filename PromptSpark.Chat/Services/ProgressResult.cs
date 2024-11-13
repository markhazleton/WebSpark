namespace PromptSpark.Chat.Services;

public class ProgressResult
{
    public bool IsError { get; }
    public string? AdaptiveCardJson { get; }
    public string? ErrorMessage { get; }

    private ProgressResult(bool isError, string? adaptiveCardJson, string? errorMessage)
    {
        IsError = isError;
        AdaptiveCardJson = adaptiveCardJson;
        ErrorMessage = errorMessage;
    }

    public static ProgressResult Success(string adaptiveCardJson) =>
        new ProgressResult(false, adaptiveCardJson, null);

    public static ProgressResult Error(string errorMessage) =>
        new ProgressResult(true, null, errorMessage);

    public static ProgressResult NoAdaptiveCard() =>
        new ProgressResult(false, null, null);
}
