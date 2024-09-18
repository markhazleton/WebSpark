namespace InquirySpark.Domain.SDK.SurveyResponse;

public interface ISurveyResponse
{
    int SurveyResponseID { get; set; }
    string SurveyResponseNM { get; set; }
    int StatusID { get; set; }
    int ApplicationID { get; set; }
    int? AssignedUserID { get; set; }
    int? AssignedSupervisorUserID { get; set; }
    string DataSource { get; set; }
    int ModifiedID { get; set; }
    string SupervisorAccountNM { get; set; }
    string AccountNM { get; set; }
    string StatusNM { get; set; }
    bool ShowQuestionDescription { get; set; }
    int PercentComplete { get; set; }
    int AnswerCount { get; set; }
    int QuestionCount { get; set; }
    int VariantAnswersCount { get; set; }
    int ComplianceReviewCount { get; set; }
    string ManagerUserID { get; set; }
    string Manager_Name { get; set; }
    string Employee_FName { get; set; }
    string Employee_LName { get; set; }
    SurveyItem Survey { get; set; }
    List<SurveyResponseSequenceItem> SequenceList { get; set; }
    List<SurveyResponseAnswerItem> AnswerList { get; set; }
    List<SurveyResponseAnswerItem> NewAnswerList { get; set; }
    List<SurveyResponseAnswerReviewItem> NewAnswerReviewList { get; set; }
    List<SurveyResponseHistoryItem> SurveyResponseHistory { get; set; }
    List<SurveyResponseStateItem> StateList { get; set; }
}