using TriviaSpark.Domain.Models;

namespace TriviaSpark.Domain.Services
{
    public interface IQuestionSourceAdapter
    {
        public Task<List<QuestionModel>> GetQuestions(int questionCount = 1, Models.Difficulty difficulty = Models.Difficulty.Easy, CancellationToken ct = default);
    }
}
