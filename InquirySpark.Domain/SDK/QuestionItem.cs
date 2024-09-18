namespace InquirySpark.Domain.SDK;

/// <summary>
/// 
/// </summary>
public class QuestionItem
{
    /// <summary>
    /// 
    /// </summary>
    public int QuestionID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int QuestionSort { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string QuestionNM { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string QuestionShortNM { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string QuestionTypeCD { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int QuestionTypeID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string ControlNM { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string AnswerDataType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool CommentFL { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int MaxQuestionValue { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string QuestionDS { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public decimal QuestionValue { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int ReviewRoleLevel { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int SurveyTypeID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int UnitOfMeasureID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public byte[] FileData { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int ModifiedID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Keywords { get; set; }
    /// <summary>
    /// Possible Answers for this Question
    /// Only Implemented when Question is in a QuestionGroup or SurveyQuestionList
    /// </summary>
    public List<QuestionAnswerItem> QuestionAnswerItemList { get; set; } = [];
    /// <summary>
    /// 
    /// </summary>
    public QuestionGroupMemberItem QuestionGroupMember { get; set; } = new QuestionGroupMemberItem();
    /// <summary>
    /// Only Implemented when Question is in a SurveyResponse.Survey.Question 
    /// </summary>
    public List<SurveyResponseAnswerItem> SurveyResponseAnswerItemList { get; set; } = [];
    /// <summary>
    /// 
    /// </summary>
    public int SurveyDisplayOrder { get; set; }
}