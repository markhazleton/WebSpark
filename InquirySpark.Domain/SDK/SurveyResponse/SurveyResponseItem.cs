using System;

namespace InquirySpark.Domain.SDK.SurveyResponse;

public class SurveyResponseItem : ISurveyResponse
{
    public int SurveyResponseID { get; set; }
    public string SurveyResponseNM { get; set; }
    public int ApplicationID { get; set; }
    public int? AssignedUserID { get; set; }
    public int? AssignedSupervisorUserID { get; set; }
    public string DataSource { get; set; }
    public int ModifiedID { get; set; }
    public string AccountNM { get; set; }
    public bool ShowQuestionDescription { get; set; } = false;
    public SurveyItem Survey { get; set; } = new SurveyItem();
    public string StatusNM { get; set; }
    public int StatusID { get; set; }
    public string SupervisorAccountNM { get; set; }
    public int AnswerCount { get; set; }
    public int ComplianceReviewCount { get; set; }
    public string Manager_Name { get; set; }
    public string Employee_FName { get; set; }
    public string Employee_LName { get; set; }
    public string ManagerUserID { get; set; }
    public int PercentComplete { get; set; }
    public int QuestionCount { get; set; }
    public int VariantAnswersCount { get; set; }
    public List<SurveyResponseAnswerItem> NewAnswerList { get; set; } = [];
    public List<SurveyResponseAnswerReviewItem> NewAnswerReviewList { get; set; } = [];
    public List<SurveyResponseHistoryItem> SurveyResponseHistory { get; set; } = [];
    public List<SurveyResponseSequenceItem> SequenceList { get; set; } = [];
    public List<SurveyResponseAnswerItem> AnswerList { get; set; } = [];
    public List<SurveyResponseStateItem> StateList { get; set; } = [];
    public int CurrentQuestionGroupID { get; set; }
    public int DaysSinceModified { get; set; }
    public decimal SurveyResponseScore { get; set; }
    public DateTime ModifiedDT { get; set; }
}