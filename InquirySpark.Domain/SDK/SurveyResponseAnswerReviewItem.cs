namespace InquirySpark.Domain.SDK;

public class SurveyResponseAnswerReviewItem
{
    public int SurveyResponseAnswerReviewID { get; set; }
    public int SurveyAnswerID { get; set; }
    public int ApplicationUserRoleID { get; set; }
    public int ReviewStatusID { get; set; }
    public int ReviewLevel { get; set; }
    public int ModifiedID { get; set; }
    public DateTime ModifiedDT { get; set; }
    public string ModifiedComment { get; set; }
    public bool ApprovedFL { get; set; }
    public string ReviewerAccountNM { get; set; }
    public string ReviewerNM { get; set; }
}