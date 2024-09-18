using InquirySpark.Domain.SDK.SurveyResponse;

namespace InquirySpark.Domain.SDK;

/// <summary>
/// ApplicationSurveyItem
/// </summary>
public class ApplicationSurveyItem
{
    /// <summary>
    /// applicationSurveyID
    /// </summary>
    public int ApplicationSurveyID { get; set; }
    /// <summary>
    /// Survey Item
    /// </summary>
    public SurveyItem Survey { get; set; } = new SurveyItem();
    /// <summary>
    /// Application Id
    /// </summary>
    public int ApplicationID { get; set; }
    /// <summary>
    /// Default Role Id
    /// </summary>
    public int DefaultRoleID { get; set; }
    /// <summary>
    /// Survey Response List
    /// </summary>
    public List<SurveyResponseItem> SurveyResponseList { get; set; } = [];
}