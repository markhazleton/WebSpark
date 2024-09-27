using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebSpark.Portal.Areas.TriviaSpark.Models.JShow;

public class QuestionViewModel
{
    public string Id { get; set; }

    [Display(Name = "Question")]
    public string QuestionText { get; set; }

    [Display(Name = "Answer")]
    public string Answer { get; set; }

    [Display(Name = "Value")]
    public int Value { get; set; }

    // Additional properties for context
    [Display(Name = "Show Number")]
    public int ShowNumber { get; set; }

    [Display(Name = "Theme")]
    public string Theme { get; set; }
    [Display(Name = "Air Date")]
    public string AirDate { get; set; }

    [Display(Name = "Round")]
    public string RoundName { get; set; }

    [Display(Name = "Category")]
    public string CategoryName { get; set; }
}
public class JShow
{
    public JShow()
    {
        Id = Guid.NewGuid().ToString();
    }

    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("show_number")]
    public int ShowNumber { get; set; }

    [JsonPropertyName("air_date")]
    public string AirDate { get; set; }

    [JsonPropertyName("theme")]
    public string Theme { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("rounds")]
    public Rounds Rounds { get; set; }

    public Question GetQuestionByID(string id)
    {
        // Search through Jeopardy round
        foreach (var category in Rounds.Jeopardy.Categories)
        {
            var question = category.Questions.Find(q => q.Id == id);
            if (question != null)
                return question;
        }

        // Search through Double Jeopardy round
        foreach (var category in Rounds.DoubleJeopardy.Categories)
        {
            var question = category.Questions.Find(q => q.Id == id);
            if (question != null)
                return question;
        }

        // Return null if no question found with the specified ID
        return null;
    }


}

public class Rounds
{
    [JsonPropertyName("jeopardy")]
    public Round Jeopardy { get; set; }

    [JsonPropertyName("double_jeopardy")]
    public Round DoubleJeopardy { get; set; }

    [JsonPropertyName("final_jeopardy")]
    public FinalJeopardy FinalJeopardy { get; set; }
}

public class Round
{
    [JsonPropertyName("theme")]
    public string Theme { get; set; }
    [JsonPropertyName("categories")]
    public List<Category> Categories { get; set; }
}

public class Category
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("questions")]
    public List<Question> Questions { get; set; }
}

public class Question
{

    public Question()
    {
         Id = Guid.NewGuid().ToString();
    }
    [JsonPropertyName("jshowid")]
    public string JShowId { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("question")]
    public string QuestionText { get; set; }

    [JsonPropertyName("answer")]
    public string Answer { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }
    [JsonPropertyName("theme")]
    public string Theme { get; set; }
}

public class FinalJeopardy
{
    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("question")]
    public string QuestionText { get; set; }

    [JsonPropertyName("answer")]
    public string Answer { get; set; }
}

