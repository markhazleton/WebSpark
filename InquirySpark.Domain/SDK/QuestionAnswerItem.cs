namespace InquirySpark.Domain.SDK;

public class QuestionAnswerItem
{
    public int QuestionID { get; set; }
    public int QuestionAnswerID { get; set; }
    public string QuestionAnswerNM { get; set; }
    public string QuestionAnswerShortNM { get; set; }
    public double QuestionAnswerValue { get; set; }
    public string QuestionAnswerDS { get; set; }
    public int QuestionAnswerSort { get; set; }
    public bool QuestionAnswerCommentFL { get; set; }
    public bool QuestionAnswerActiveFL { get; set; }
    public bool MarkedForDeletion { get; set; } = false;
    public DateTime ModifiedDT { get; set; }
    public int ModifedID { get; set; }
}