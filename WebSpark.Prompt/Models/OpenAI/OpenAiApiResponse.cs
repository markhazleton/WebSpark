namespace WebSpark.Prompt.Models.OpenAI;

public class OpenAiApiResponse
{
    public string Id { get; set; }
    public string Object { get; set; }
    public int Created { get; set; }
    public string Model { get; set; }
    public List<Choice> Choices { get; set; }
    public Usage usage { get; set; }
    public string SystemFingerprint { get; set; }
}
