
namespace InquirySpark.Domain.SDK;

public class SurveyStatusItem
{
    public int SurveyStatusID { get; set; }
    public int SurveyID { get; set; }
    public int StatusID { get; set; }
    public string StatusNM { get; set; }
    public string StatusDS { get; set; }
    public string SubjectTemplate { get; set; }
    public string BodyTemplate { get; set; }
    public int ModifiedID { get; set; }
    public int NextStatusID { get; set; }
    public int PreviousStatusID { get; set; }
}