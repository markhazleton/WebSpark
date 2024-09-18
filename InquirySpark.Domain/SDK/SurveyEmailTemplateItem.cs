namespace InquirySpark.Domain.SDK;

public class SurveyEmailTemplateItem
{
    public int SurveyEmailTemplateID { get; set; }
    public string SurveyEmailTemplateNM { get; set; }
    public string BodyTemplate { get; set; }
    public string SubjectTemplate { get; set; }
    public string FromEmailAddress { get; set; }
    public string FilterCriteria { get; set; }
    public int SurveyID { get; set; }
    public int StatusID { get; set; }
    public DateTime? StartDT { get; set; }
    public DateTime? EndDT { get; set; }
    public bool IsActive { get; set; }
    public bool SendToSupervisor { get; set; }
    public int ModifiedID { get; set; }
}