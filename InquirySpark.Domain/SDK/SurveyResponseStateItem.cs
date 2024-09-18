namespace InquirySpark.Domain.SDK;

public class SurveyResponseStateItem
{
    public int SurveyResponseStateID { get; set; }
    public int SurveyResponseID { get; set; }
    public int StatusID { get; set; }
    public int AssignedUserID { get; set; }
    public bool Active { get; set; }
    public bool EmailSent { get; set; }
    public string EmailBody { get; set; }
    public int ModifiedID { get; set; }
    public DateTime ModifiedDT { get; set; }
}