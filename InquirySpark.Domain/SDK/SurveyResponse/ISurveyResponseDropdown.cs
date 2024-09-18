
namespace InquirySpark.Domain.SDK.SurveyResponse;

public interface ISurveyResponseDropdown
{
    int SurveyResponseID { get; }
    bool ForInput { get; set; }
}