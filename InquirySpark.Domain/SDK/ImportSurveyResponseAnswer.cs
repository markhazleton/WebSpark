
namespace InquirySpark.Domain.SDK;

public class ImportSurveyResponseAnswer
{
    public int QuestionID { get; set; }
    public string QuestionNM { get; set; }
    public string Response { get; set; }
    public string ResponderComment { get; set; }
    public string ReviewerComment { get; set; }
    public string ReviewCompletedBy { get; set; }
    public string ReviewCompletedDate { get; set; }
}