using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TriviaSpark.JShow.Data;

namespace TriviaSpark.JShow.Models;
public class RoundVM
{
    public RoundVM()
    {
        Id = Guid.NewGuid().ToString();
    }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("showid")]
    public string JShowId { get; set; }
    public string Name { get; set; }
    [JsonPropertyName("theme")]
    public string Theme { get; set; }
    [JsonPropertyName("categories")]
    public List<CategoryVM> Categories { get; set; }
}
public class CategoryVM
{
    public CategoryVM()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string Id { get; set; }
    [JsonPropertyName("roundid")]
    public string RoundId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("questions")]
    public List<QuestionVM> Questions { get; set; }
}
public class QuestionVM
{
    public QuestionVM()
    {
        Id = Guid.NewGuid().ToString();
    }
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("jshowid")]
    public string JShowId { get; set; }

    [Display(Name = "Question")]
    [JsonPropertyName("question")]
    public string QuestionText { get; set; }

    [Display(Name = "Answer")]
    [JsonPropertyName("answer")]
    public string Answer { get; set; }

    [Display(Name = "Value")]
    [JsonPropertyName("value")]
    public int Value { get; set; }

    // Additional properties for context
    [Display(Name = "Show Number")]
    public int ShowNumber { get; set; }

    [Display(Name = "Theme")]
    [JsonPropertyName("theme")]
    public string Theme { get; set; }
    [Display(Name = "Air Date")]
    public string AirDate { get; set; }

    [Display(Name = "Round")]
    [JsonPropertyName("round")]
    public string RoundName { get; set; }

    [Display(Name = "Category")]
    [JsonPropertyName("category")]
    public string CategoryName { get; set; }
    public string CategoryId { get; set; }
}
public class JShowVM
{
    public JShowVM()
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
    public Rounds? Rounds { get; set; }
    public JShowType Type { get; set; }
    public QuestionVM GetQuestionByID(string id)
    {
        foreach (var category in Rounds.Jeopardy.Categories)
        {
            var question = category.Questions.Find(q => q.Id == id);
            if (question != null)
                return question;
        }
        foreach (var category in Rounds.DoubleJeopardy.Categories)
        {
            var question = category.Questions.Find(q => q.Id == id);
            if (question != null)
                return question;
        }
        return new QuestionVM();
    }
}

public class Rounds
{
    [JsonPropertyName("jeopardy")]
    public RoundVM Jeopardy { get; set; }

    [JsonPropertyName("double_jeopardy")]
    public RoundVM DoubleJeopardy { get; set; }

    [JsonPropertyName("final_jeopardy")]
    public FinalJeopardy FinalJeopardy { get; set; }
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

