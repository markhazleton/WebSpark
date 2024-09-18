namespace InquirySpark.Domain.SDK;

public class SurveyResponseHistoryItem
{
    public int SurveyResponseHistoryID { get; set; }
    public int SurveyResponseID { get; set; }
    public string SurveyResponseNM { get; set; }
    public int ApplicationUserID { get; set; }
    public int? QuestionGroupID { get; set; }
    public int StatusID { get; set; }
    public string UserNM { get; set; }
    public string Answers { get; set; }
    public DateTime ModifiedDT { get; set; }
    public int ModifiedID { get; set; }
    public string ModifiedNM { get; set; }
    public string StatusNM { get; set; }
}