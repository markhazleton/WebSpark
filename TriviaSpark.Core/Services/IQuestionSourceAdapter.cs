namespace TriviaSpark.Core.Services
{
    public interface IQuestionSourceAdapter
    {
        public Task<List<Models.QuestionModel>> GetQuestions(int questionCount = 1, Models.Difficulty difficulty = Models.Difficulty.Easy, CancellationToken ct = default);
    }
}
