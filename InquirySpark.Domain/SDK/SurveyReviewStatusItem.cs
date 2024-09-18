
namespace InquirySpark.Domain.SDK;

public class SurveyReviewStatusItem
{
    public int SurveyReviewStatusID { get; set; }
    public int SurveyID { get; set; }
    public string ReviewStatusDS { get; set; }
    public int ReviewStatusID { get; set; }
    public string ReviewStatusNM { get; set; }
    public bool CommentFL { get; set; }
    public bool ApprovedFL { get; set; }
    public int ModifiedID { get; set; }
}