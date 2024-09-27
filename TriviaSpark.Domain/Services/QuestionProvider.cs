using TriviaSpark.Domain.Models;
using TriviaSpark.Domain.Utility;

namespace TriviaSpark.Domain.Services;


/// <summary>
/// Represents a provider of items.
/// </summary>
public class QuestionProvider : ListProvider<QuestionModel>
{
    private List<string> GetQuestionsWithoutCorrectResponse(List<QuestionAnswerSet> questions, List<QuestionAnswerSet> answered)
    {
        var unansweredQuestions = new List<string>();
        foreach (var question in questions)
        {
            var hasAnswer = answered.Any(a => a.QuestionId == question.QuestionId);
            if (!hasAnswer)
            {
                unansweredQuestions.Add(question.QuestionId);
            }
        }
        return unansweredQuestions;
    }

    public ScoreCardModel CalculateScore(IEnumerable<MatchQuestionAnswerModel> matchQuestionAnswers)
    {
        List<QuestionAnswerSet> unansweredQuestions = [];
        var correctAnswersByQuestion = CorrectAnswersByQuestion();
        var attemptedAnswersByQuestion = AttemptedAnswersByQuestion(matchQuestionAnswers);
        // Calculate the number of questions and number of questions answers
        double runningScore = 0.00;
        int numCorrect = 0;
        int numInCorrectAnswers = 0;
        foreach (var questionId in correctAnswersByQuestion.Keys)
        {
            int correctAnserId = correctAnswersByQuestion[questionId].FirstOrDefault();
            if (!attemptedAnswersByQuestion.ContainsKey(questionId))
            {
                // Question not attempted
                unansweredQuestions.Add(new QuestionAnswerSet { QuestionId = questionId, AnswerId = 0 });
            }
            else if (attemptedAnswersByQuestion[questionId].TryGetValue(correctAnserId, out int correctAttemptId))
            {
                var questionAnswers = attemptedAnswersByQuestion[questionId].Count;
                runningScore += 1.00 - (questionAnswers - 1) * .2;
                // Question attempted and questions 
                numCorrect++;
                numInCorrectAnswers += questionAnswers - 1;
            }
            else
            {
                numInCorrectAnswers += attemptedAnswersByQuestion[questionId].Count;
            }
        }
        // Create the score card object
        return new ScoreCardModel
        {
            PercentCorrect = Math.Round((double)numCorrect / Items.Count(), 2),
            AdjustedScore = runningScore / Items.Count(),
            QuestionCount = Items.Count(),
            QuestionsAttempted = attemptedAnswersByQuestion.Count,
            CorrectAnswers = numCorrect,
            IncorrectAnswers = numInCorrectAnswers,
        };
    }

    private static Dictionary<string, HashSet<int>> AttemptedAnswersByQuestion(IEnumerable<MatchQuestionAnswerModel> matchQuestionAnswers)
    {

        // Create a dictionary of attempted answer IDs for each question
        return matchQuestionAnswers
            .Select(a => new QuestionAnswerSet()
            {
                QuestionId = a.QuestionId,
                AnswerId = a.AnswerId
            }).GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => new HashSet<int>(g.Select(a => a.AnswerId)));
    }

    public Dictionary<string, HashSet<int>> CorrectAnswersByQuestion()
    {
        var results = new Dictionary<string, HashSet<int>>();

        if (!Items.Any(Items => Items.Answers is not null))
        {
            return results;
        }

        // Create a dictionary of questions answer IDs for each question
        return Items
            .SelectMany(q => q.Answers)
            .Where(a => a.IsCorrect)
            .Select(a => new QuestionAnswerSet()
            {
                QuestionId = a.QuestionId,
                AnswerId = a.AnswerId
            }).GroupBy(c => c.QuestionId)
            .ToDictionary(g => g.Key, g => new HashSet<int>(g.Select(a => a.AnswerId)));
    }

    public List<QuestionModel> GetCorrectQuestions(IEnumerable<MatchQuestionAnswerModel> matchQuestionAnswers)
    {
        var correctAnswerIds = Items.SelectMany(q => q.Answers)
                                         .Where(a => a.IsCorrect)
                                         .Select(a => a.AnswerId)
                                         .ToList();
        var answerIds = matchQuestionAnswers.Select(s => s.AnswerId).ToList();
        return Items.Where(q => q.Answers.Any(a => answerIds.Contains(a.AnswerId) && a.IsCorrect) == true)
                        .ToList();
    }

    public List<QuestionModel> GetIncorrectQuestions(IEnumerable<MatchQuestionAnswerModel> matchQuestionAnswers)
    {
        if (!matchQuestionAnswers.Any()) return [];

        if (!Items.Any(Items => Items.Answers is not null))
        {
            return [];
        }
        var correctAnswers = Items
            .SelectMany(q => q.Answers)
            .Where(a => a.IsCorrect)
            .Select(a => new QuestionAnswerSet()
            {
                QuestionId = a.QuestionId,
                AnswerId = a.AnswerId
            }).ToList();

        var unansweredQuestions = new List<string>();
        foreach (var question in correctAnswers)
        {
            var hasAnswer = matchQuestionAnswers.Any(a => a.QuestionId == question.QuestionId && a.AnswerId == question.AnswerId);
            if (!hasAnswer)
            {
                unansweredQuestions.Add(question.QuestionId);
            }
        }
        List<QuestionModel> result = Items.Where(w => unansweredQuestions.Contains(w.QuestionId)).ToList();
        return result;
    }
    public List<QuestionModel> GetAttemptedQuestions(IEnumerable<MatchQuestionAnswerModel> matchQuestionAnswers)
    {
        return Items.Where(q => matchQuestionAnswers.Select(s => s.QuestionId).Distinct().Contains(q.QuestionId)).ToList();
    }
    public List<QuestionModel> GetUnansweredQuestions(IEnumerable<MatchQuestionAnswerModel> matchQuestionAnswers)
    {
        return Items.Where(q => !matchQuestionAnswers.Select(s => s.QuestionId).Distinct().Contains(q.QuestionId)).ToList();
    }
    internal class QuestionAnswerSet
    {
        public int AnswerId { get; set; }
        public string QuestionId { get; set; } = string.Empty;
    }
}
