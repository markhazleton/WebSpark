
namespace InquirySpark.Domain.SDK;

/// <summary>
/// 
/// </summary>
public class QuestionGroupMemberItem
{
    /// <summary>
    /// 
    /// </summary>
    public int QuestionGroupMemberID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int QuestionGroupID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int QuestionID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int DisplayOrder { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double QuestionWeight { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string QuestionGroupNM { get; set; }
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
    public string QuestionGroupShortNM { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int ModifiedID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool MarkedForDeletion { get; set; } = false;
    /// <summary>
    /// 
    /// </summary>
    public QuestionItem? Question { get; set; }
}