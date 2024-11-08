namespace PromptSpark.Domain.Models.OpenAI
{
    public class OpenAiApiRequest
    {
        public string model { get; set; }
        // public ResponseFormat response_format { get; set; }
        public List<Message> messages { get; set; }
        public double temperature { get; set; }
    }
}
