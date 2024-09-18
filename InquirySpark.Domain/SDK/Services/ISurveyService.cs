using InquirySpark.Domain.Models;

namespace InquirySpark.Domain.SDK.Services;

/// <summary>
/// Interface ISurveyService
/// </summary>
public interface ISurveyService
{
    /// <summary>
    /// Gets the application by application identifier.
    /// </summary>
    /// <param name="ApplicationId">The application identifier.</param>
    /// <returns>ApplicationItem.</returns>
    Task<BaseResponse<ApplicationItem>> GetApplicationByApplicationID(int ApplicationId);

    /// <summary>
    /// Gets the application type by application type identifier.
    /// </summary>
    /// <param name="applicationType">Type of the application.</param>
    /// <returns>ApplicationTypeItem.</returns>
    Task<BaseResponse<ApplicationTypeItem>> GetApplicationTypeByApplicationTypeID(ApplicationTypeItem applicationType);

    /// <summary>
    /// Gets the application type by application type identifier.
    /// </summary>
    /// <param name="applicationTypeId">The application type identifier.</param>
    /// <returns>ApplicationTypeItem.</returns>
    Task<BaseResponse<ApplicationTypeItem>> GetApplicationTypeByApplicationTypeID(int applicationTypeId);

    /// <summary>
    /// Gets the application type collection.
    /// </summary>
    /// <returns>ApplicationTypeItem[].</returns>
    Task<BaseResponseCollection<ApplicationTypeItem>> GetApplicationTypeCollection();

    /// <summary>
    /// Gets the company by company identifier.
    /// </summary>
    /// <param name="CompanyId">The company identifier.</param>
    /// <returns>CompanyItem.</returns>
    Task<BaseResponse<CompanyItem>> GetCompanyByCompanyId(int CompanyId);

    /// <summary>
    /// Gets the company collection.
    /// </summary>
    /// <returns>CompanyItem[].</returns>
    Task<BaseResponseCollection<CompanyItem>> GetCompanyCollection();
    /// <summary>
    /// Gets the question by question identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<BaseResponse<QuestionItem>> GetQuestionByQuestionId(int id);

    /// <summary>
    /// Gets the survey by survey identifier.
    /// </summary>
    /// <param name="surveyId">The survey identifier.</param>
    /// <returns>SurveyItem.</returns>
    Task<BaseResponse<SurveyItem>> GetSurveyBySurveyId(int surveyId);

    /// <summary>
    /// Gets the survey collection.
    /// </summary>
    /// <returns>SurveyItem[].</returns>
    Task<BaseResponseCollection<SurveyItem>> GetSurveyCollection();

    /// <summary>
    /// Gets the type of the survey.
    /// </summary>
    /// <param name="surveyTypeId">The survey type identifier.</param>
    /// <returns>SurveyTypeItem.</returns>
    Task<BaseResponse<SurveyTypeItem>> GetSurveyType(int surveyTypeId);

    /// <summary>
    /// Gets the survey type collection.
    /// </summary>
    /// <param name="surveyTypeId">The survey type identifier.</param>
    /// <returns>SurveyTypeItem[].</returns>
    Task<BaseResponseCollection<SurveyTypeItem>> GetSurveyTypeCollection(int surveyTypeId);

    /// <summary>
    /// Gets the user by identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>ApplicationUserItem.</returns>
    Task<BaseResponse<ApplicationUserItem>> GetUserById(int Id);

    /// <summary>
    /// Gets the user collection.
    /// </summary>
    /// <returns>ApplicationUserItem[].</returns>
    Task<BaseResponseCollection<ApplicationUserItem>> GetUserCollection();

    /// <summary>
    /// Puts the application.
    /// </summary>
    /// <param name="applicationItem">The application item.</param>
    /// <returns>ApplicationItem.</returns>
    Task<BaseResponse<ApplicationItem>> PutApplication(ApplicationItem applicationItem);

    /// <summary>
    /// Puts the company.
    /// </summary>
    /// <param name="company">The company.</param>
    /// <returns>CompanyItem.</returns>
    Task<BaseResponse<CompanyItem>> PutCompany(CompanyItem company);

    /// <summary>
    /// Puts the survey.
    /// </summary>
    /// <param name="survey">The survey.</param>
    /// <returns>SurveyItem.</returns>
    Task<BaseResponse<SurveyItem>> PutSurvey(SurveyItem survey);

    /// <summary>
    /// Puts the user.
    /// </summary>
    /// <param name="userItem">The user item.</param>
    /// <returns>ApplicationUserItem.</returns>
    Task<BaseResponse<ApplicationUserItem>> PutUser(ApplicationUserItem userItem);
}
