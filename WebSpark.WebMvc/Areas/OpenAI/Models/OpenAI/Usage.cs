namespace PromptSpark.Areas.OpenAI.Models.OpenAI;

public class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    /// <summary>
    /// The total number of tokens that were used in processing the completion request.
    /// </summary>
    public int total_tokens { get; set; }
}
