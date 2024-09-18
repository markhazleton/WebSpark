namespace InquirySpark.Domain.SDK;

public class SurveyItem
{
    public int SurveyID { get; set; }
    public string SurveyNM { get; set; }
    public string SurveyDS { get; set; }
    public string SurveyShortNM { get; set; }
    public string CompletionMessage { get; set; }
    public string AutoAssignFilter { get; set; }
    public string ResponseNMTemplate { get; set; }
    public string ReviewerAccountNM { get; set; }
    public bool UseSurveyGroupsFL { get; set; }
    public DateTime? EndDT { get; set; }
    public DateTime? StartDT { get; set; }
    public int ModifiedID { get; set; }
    public int? ParentSurveyID { get; set; }
    public List<QuestionGroupItem> QuestionGroupList { get; set; } = [];
    public List<QuestionItem> QuestionList { get; set; } = [];
    public List<SurveyEmailTemplateItem> EmailTemplateList { get; set; } = [];
    public List<SurveyReviewStatusItem> ReviewStatusList { get; set; } = [];
    public List<SurveyStatusItem> StatusList { get; set; } = [];
    public SurveyTypeItem SurveyType { get; set; } = new SurveyTypeItem();
    public int ApplicationCount { get; set; }
    public int SurveyResponseCount { get; set; }
    public int QuestionCount { get; set; }
    public int QuestionGroupCount { get; set; }
}