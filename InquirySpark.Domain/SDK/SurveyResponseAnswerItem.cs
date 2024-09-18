namespace InquirySpark.Domain.SDK;

public class SurveyResponseAnswerItem
{
    public string DisplayAnswerNM { get; set; }
    public string DisplayAnswerComment { get; set; }
    public int SurveyAnswerID { get; set; }
    public int SurveyResponseID { get; set; }
    public int SequenceNumber { get; set; }
    public int QuestionID { get; set; }
    public int QuestionAnswerID { get; set; }
    public string AnswerType { get; set; }
    public double AnswerQuantity { get; set; }
    public DateTime? AnswerDate { get; set; }
    public string AnswerComment { get; set; }
    public string ModifiedComment { get; set; }
    public int ModifiedID { get; set; }
    public DateTime ModifiedDT { get; set; }
    public string QuestionAnswerNM { get; set; }
    public decimal QuestionValue { get; set; }
    public decimal QuestionAnswerValue { get; set; }
    public List<string> ResponseList { get; set; } = [];
    public List<SurveyResponseAnswerReviewItem> AnswerReviewList { get; set; } = [];
}