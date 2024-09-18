namespace InquirySpark.Domain.SDK;

public class SurveyTypeItem
{
    public int SurveyTypeID { get; set; }
    public string SurveyTypeShortNM { get; set; }
    public string SurveyTypeNM { get; set; }
    public string SurveyTypeDS { get; set; }
    public string SurveyTypeComment { get; set; }
    public int ApplicationTypeID { get; set; }
    public string ApplicationTypeNM { get; set; }
    public bool MultiSequence { get; set; }
    public int ParentSurveyTypeID { get; set; }
    public string ParentSurveyTypeNM { get; set; }
    public int LevelNumber { get; set; }
    public string TreeSort { get; set; }
    public int ModifiedID { get; set; }
    public DateTime ModifiedDT { get; set; }
    public int QuestionCount { get; set; }
    public int SurveyCount { get; set; }
    public int ChildCount { get; set; }
}