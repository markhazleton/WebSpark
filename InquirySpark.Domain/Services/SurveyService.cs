using InquirySpark.Domain.Database;
using InquirySpark.Domain.Models;
using InquirySpark.Domain.SDK;
using InquirySpark.Domain.SDK.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InquirySpark.Domain.Services;

/// <summary>
/// Survey Service
/// </summary>
/// <remarks>
/// Survey Service Constructor
/// </remarks>
/// <param name="coSurveyContext"></param>
/// <param name="logger"></param>
public class SurveyService(InquirySparkDbContext coSurveyContext, ILogger<SurveyService> logger) : ISurveyService
{

    /// <summary>
    /// GetApplicationByApplicationID
    /// </summary>
    /// <param name="ApplicationId"></param>
    /// <returns></returns>
    public async Task<BaseResponse<ApplicationItem>> GetApplicationByApplicationID(int ApplicationId)
    {
        _logger.LogInformation("GetApplicationByApplicationID");
        return await DbContextHelper.ExecuteAsync(async () =>
        {
            return await _coSurveyContext
                .Applications
                .Where(w => w.ApplicationId == ApplicationId)
                .Include(i => i.ApplicationType)
                .Include(i => i.Company)
                .Include(i => i.SurveyResponses)
                .Include(i => i.ApplicationUserRoles).ThenInclude(i => i.ApplicationUser)
                .Include(i => i.ApplicationSurveys).ThenInclude(i => i.Survey)
                                                   .ThenInclude(i => i.QuestionGroups)
                                                   .ThenInclude(i => i.QuestionGroupMembers)
                                                   .ThenInclude(i => i.Question)
                                                   .ThenInclude(i => i.QuestionAnswers)
                .Select(s => SurveyServices_Mappers.Create(s))
                .FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Get Application By Application Type 
    /// </summary>
    /// <param name="applicationType"></param>
    /// <returns></returns>
    public async Task<BaseResponse<ApplicationTypeItem>> GetApplicationTypeByApplicationTypeID(ApplicationTypeItem applicationType)
    {
        return await DbContextHelper.ExecuteAsync(async () =>
        {
            return await _coSurveyContext
                .LuApplicationTypes
                .Where(w => w.ApplicationTypeId == applicationType.ApplicationTypeID)
                .Select(s => SurveyServices_Mappers.Create(s))
                .FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Get Application By Application Type ID
    /// </summary>
    /// <param name="applicationTypeId"></param>
    /// <returns></returns>
    public async Task<BaseResponse<ApplicationTypeItem>> GetApplicationTypeByApplicationTypeID(int applicationTypeId)
    {
        return await DbContextHelper.ExecuteAsync(async () =>
        {
            return await _coSurveyContext
                .LuApplicationTypes
                .Where(w => w.ApplicationTypeId == applicationTypeId)
                .Select(s => SurveyServices_Mappers.Create(s))
                .FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Get Application Type Collection
    /// </summary>
    /// <returns></returns>
    public async Task<BaseResponseCollection<ApplicationTypeItem>> GetApplicationTypeCollection()
    {
        return await DbContextHelper.ExecuteCollectionAsync<ApplicationTypeItem>(async () =>
        {
            return await _coSurveyContext
            .LuApplicationTypes
            .Select(s => SurveyServices_Mappers.Create(s))
            .ToListAsync();
        });
    }

    /// <summary>
    /// Get Company By CompanyId
    /// </summary>
    /// <param name="CompanyId"></param>
    /// <returns></returns>
    public async Task<BaseResponse<CompanyItem>> GetCompanyByCompanyId(int CompanyId)
    {
        return await DbContextHelper.ExecuteAsync(async () =>
        {
            return await _coSurveyContext
            .Companies.Where(w => w.CompanyId == CompanyId)
            .Include(i => i.Applications)
            .Include(i => i.ApplicationUsers)
            .Select(s => SurveyServices_Mappers.Create(s))
            .FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Get Company Collection
    /// </summary>
    /// <returns></returns>
    public async Task<BaseResponseCollection<CompanyItem>> GetCompanyCollection()
    {
        return await DbContextHelper.ExecuteCollectionAsync<CompanyItem>(async () =>
        {
            return await _coSurveyContext
            .Companies
            .Include(i => i.Applications)
            .Select(s => SurveyServices_Mappers.Create(s))
            .ToListAsync();
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<BaseResponse<QuestionItem>> GetQuestionByQuestionId(int id)
    {
        return await DbContextHelper.ExecuteAsync<QuestionItem>(async () =>
            {
                return await _coSurveyContext
                .Questions
                .Where(w => w.QuestionId == id)
                .Include(i => i.QuestionAnswers)
                .Select(s => SurveyServices_Mappers.Create(s))
                .FirstOrDefaultAsync();
            });
    }

    /// <summary>
    /// Get Survey By SurveyId
    /// </summary>
    /// <param name="surveyId"></param>
    /// <returns></returns>
    public async Task<BaseResponse<SurveyItem>> GetSurveyBySurveyId(int surveyId)
    {
        return await DbContextHelper.ExecuteAsync<SurveyItem>(async () =>
        {
            return await _coSurveyContext
            .Surveys.Where(w => w.SurveyId == surveyId)
            .Include(i => i.QuestionGroups).ThenInclude(i => i.QuestionGroupMembers).ThenInclude(i => i.Question)
            .Select(s => SurveyServices_Mappers.Create(s))
            .FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Get survey collection
    /// </summary>
    /// <returns></returns>
    public async Task<BaseResponseCollection<SurveyItem>> GetSurveyCollection()
    {
        return await DbContextHelper.ExecuteCollectionAsync<SurveyItem>(async () =>
        {
            var list = await _coSurveyContext
            .Surveys
            .Select(s => SurveyServices_Mappers.Create(s))
            .ToListAsync();

            return list;
        });
    }

    /// <summary>
    /// Get survey type by survey type id
    /// </summary>
    /// <param name="surveyTypeId"></param>
    /// <returns></returns>
    public async Task<BaseResponse<SurveyTypeItem>> GetSurveyType(int surveyTypeId)
    {
        return await DbContextHelper.ExecuteAsync<SurveyTypeItem>(async () =>
        {
            return await _coSurveyContext
            .LuSurveyTypes.Where(w => w.SurveyTypeId == surveyTypeId)
            .Select(s => SurveyServices_Mappers.Create(s))
            .FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Get survey type collection
    /// </summary>
    /// <param name="surveyTypeId"></param>
    /// <returns></returns>
    public async Task<BaseResponseCollection<SurveyTypeItem>> GetSurveyTypeCollection(int surveyTypeId)
    {
        return await DbContextHelper.ExecuteCollectionAsync<SurveyTypeItem>(async () =>
        {
            return await _coSurveyContext
            .LuSurveyTypes
            .Select(s => SurveyServices_Mappers.Create(s))
            .ToListAsync();
        });
    }

    /// <summary>
    /// Get User By Id
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    public async Task<BaseResponse<ApplicationUserItem>> GetUserById(int Id)
    {
        return await DbContextHelper.ExecuteAsync<ApplicationUserItem>(async () =>
        {
            return await _coSurveyContext
            .ApplicationUsers.Where(w => w.ApplicationUserId == Id)
            .Select(s => SurveyServices_Mappers.Create(s))
            .FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Get User collection
    /// </summary>
    /// <returns></returns>
    public async Task<BaseResponseCollection<ApplicationUserItem>> GetUserCollection()
    {
        return await DbContextHelper.ExecuteCollectionAsync<ApplicationUserItem>(async () =>
        {
            return await _coSurveyContext
            .ApplicationUsers
            .Select(s => SurveyServices_Mappers.Create(s))
            .ToListAsync();
        });
    }

    /// <summary>
    /// Put Application
    /// </summary>
    /// <param name="applicationItem"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<BaseResponse<ApplicationItem>> PutApplication(ApplicationItem applicationItem)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Put Company
    /// </summary>
    /// <param name="company"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<BaseResponse<CompanyItem>> PutCompany(CompanyItem company)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Put Survey
    /// </summary>
    /// <param name="survey"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<BaseResponse<SurveyItem>> PutSurvey(SurveyItem survey)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Put User 
    /// </summary>
    /// <param name="userItem"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<BaseResponse<ApplicationUserItem>> PutUser(ApplicationUserItem userItem)
    {
        throw new NotImplementedException();
    }

    private readonly InquirySparkDbContext _coSurveyContext = coSurveyContext;
    private readonly ILogger<SurveyService> _logger = logger;
}
