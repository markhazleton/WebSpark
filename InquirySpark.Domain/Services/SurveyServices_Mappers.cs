using InquirySpark.Domain.Database;
using InquirySpark.Domain.SDK;

namespace InquirySpark.Domain.Services;

/// <summary>
/// SurveyServices_Mappers
/// </summary>
public static class SurveyServices_Mappers
{
    /// <summary>
    /// Mapper for QuestionGroup
    /// </summary>
    /// <param name="questionGroups"></param>
    /// <returns></returns>
    public static List<QuestionGroupItem> Create(ICollection<QuestionGroup> questionGroups)
    {
        List<QuestionGroupItem> questionGroupList = [];
        if (questionGroups == null) return questionGroupList;

        foreach (var item in questionGroups)
        {
            questionGroupList.Add(Create(item));
        }
        return questionGroupList;
    }

    /// <summary>
    /// Mapper for QuestionGroup
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static QuestionGroupItem Create(QuestionGroup item)
    {
        return new QuestionGroupItem()
        {
            QuestionGroupID = item.QuestionGroupId,
            SurveyID = item.SurveyId,
            QuestionGroupNM = item.QuestionGroupNm,
            QuestionGroupShortNM = item.QuestionGroupShortNm,
            QuestionGroupDS = item.QuestionGroupDs ?? string.Empty,
            QuestionGroupOrder = item.GroupOrder,
            QuestionGroupWeight = item.QuestionGroupWeight,
            QuestionGroupHeader = item.GroupHeader ?? string.Empty,
            QuestionGroupFooter = item.GroupFooter ?? string.Empty,
            DependentMaxScore = item.DependentMaxScore ?? 0,
            DependentMinScore = item.DependentMinScore ?? 0,
            DependentQuestionGroupID = item.DependentQuestionGroupId ?? 0,
            QuestionMembership = Create(item.QuestionGroupMembers),
            ModifiedID = item.ModifiedId,
            MarkedForDeletion = false
        };
    }

    /// <summary>
    /// Mapper for QuestionGroupMember
    /// </summary>
    /// <param name="questionGroups"></param>
    /// <returns></returns>
    public static List<QuestionGroupMemberItem> Create(ICollection<QuestionGroupMember> questionGroups)
    {
        List<QuestionGroupMemberItem> questionGroupList = [];
        if (questionGroups == null) return questionGroupList;

        foreach (var questionGroup in questionGroups)
        {
            questionGroupList.Add(Create(questionGroup));
        }
        return questionGroupList;
    }

    /// <summary>
    /// Mapper for QuestionGroupMember
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static QuestionGroupMemberItem Create(QuestionGroupMember item)
    {
        return new QuestionGroupMemberItem()
        {
            QuestionGroupMemberID = item.QuestionGroupMemberId,
            QuestionGroupID = item.QuestionGroupId,
            QuestionID = item.QuestionId,
            DisplayOrder = item.DisplayOrder,
            QuestionWeight = (double)item.QuestionWeight,
            QuestionGroupNM = item.QuestionGroup?.QuestionGroupNm ?? string.Empty,
            QuestionNM = item.Question?.QuestionNm ?? string.Empty,
            QuestionGroupShortNM = item.QuestionGroup?.QuestionGroupShortNm ?? string.Empty,
            QuestionShortNM = item.Question?.QuestionShortNm ?? string.Empty,
            ModifiedID = item.ModifiedId,
            MarkedForDeletion = false,
            Question = Create(item.Question)
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="question"></param>
    /// <returns></returns>
    public static QuestionItem Create(Question? question)
    {
        if (question == null) return new();
        return new QuestionItem()
        {
            QuestionID = question.QuestionId,
            QuestionTypeID = question.QuestionTypeId,
            CommentFL = question.CommentFl,
            QuestionDS = question.QuestionDs ?? string.Empty,
            QuestionValue = question.QuestionValue,
            ReviewRoleLevel = question.ReviewRoleLevel,
            SurveyTypeID = question.SurveyTypeId,
            UnitOfMeasureID = question.UnitOfMeasureId,
            FileData = question.FileData ?? [],
            ModifiedID = question.ModifiedId,
            Keywords = question.Keywords ?? string.Empty,
            QuestionAnswerItemList = Create(question.QuestionAnswers),
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="questionAnswers"></param>
    /// <returns></returns>
    public static List<QuestionAnswerItem> Create(ICollection<QuestionAnswer> questionAnswers)
    {
        List<QuestionAnswerItem> questionAnswerList = [];
        if (questionAnswers == null) return questionAnswerList;
        return questionAnswers.Select(s => Create(s)).ToList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="questionAnswer"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static QuestionAnswerItem Create(QuestionAnswer questionAnswer)
    {
        return new QuestionAnswerItem()
        {
            QuestionAnswerID = questionAnswer.QuestionAnswerId,
            QuestionID = questionAnswer.QuestionId,
            QuestionAnswerNM = questionAnswer.QuestionAnswerNm,
            QuestionAnswerShortNM = questionAnswer.QuestionAnswerShortNm,
            QuestionAnswerValue = questionAnswer.QuestionAnswerValue,
            QuestionAnswerDS = questionAnswer.QuestionAnswerDs ?? string.Empty,
            QuestionAnswerSort = questionAnswer.QuestionAnswerSort,
            QuestionAnswerCommentFL = questionAnswer.CommentFl,
            QuestionAnswerActiveFL = questionAnswer.ActiveFl,
            ModifiedDT = questionAnswer.ModifiedDt,
            ModifedID = questionAnswer.ModifiedId,
            MarkedForDeletion = false
        };
    }

    /// <summary>
    /// Mapper for Question
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static ApplicationItem Create(Application item)
    {
        return new ApplicationItem
        {
            ApplicationID = item.ApplicationId,
            ApplicationNM = item.ApplicationNm,
            ApplicationCD = item.ApplicationCd,
            ApplicationShortNM = item.ApplicationShortNm,
            ApplicationTypeID = item.ApplicationTypeId,
            ApplicationDS = item.ApplicationDs ?? string.Empty,
            MenuOrder = item.MenuOrder,
            ApplicationFolder = item.ApplicationFolder,
            DefaultAppPage = item.DefaultPageId,
            CompanyID = item.CompanyId ?? 0,
            ModifiedID = item.ModifiedId,
            ModifiedDT = item.ModifiedDt,
            ApplicationTypeNM = item.ApplicationType?.ApplicationTypeNm ?? "unknown",
            ApplicationSurveyList = Create(item.ApplicationSurveys),
            ApplicationUserList = Create(item.ApplicationUserRoles),
        };
    }

    public static List<ApplicationUserRoleItem> Create(ICollection<ApplicationUserRole> applicationUserRoles)
    {
        return applicationUserRoles.Select(s => Create(s)).ToList();
    }

    public static ApplicationUserRoleItem Create(ApplicationUserRole s)
    {
        return new ApplicationUserRoleItem
        {
            ApplicationUserID = s.ApplicationUserId,
            ApplicationID = s.ApplicationId,
            RoleID = s.RoleId,
            RoleNM = s.Role?.RoleNm ?? string.Empty,
            RoleDS = s.Role?.RoleDs ?? string.Empty,
            ModifiedID = s.ModifiedId
        };
    }

    /// <summary>
    /// Mapper for ApplicationSurvey
    /// </summary>
    /// <param name="applicationSurveys"></param>
    /// <returns></returns>
    public static List<ApplicationSurveyItem> Create(ICollection<ApplicationSurvey>? applicationSurveys)
    {
        List<ApplicationSurveyItem> applicationSurveyItems = [];
        if (applicationSurveys == null) return applicationSurveyItems;

        foreach (var item in applicationSurveys)
        {
            applicationSurveyItems.Add(Create(item));
        }
        return applicationSurveyItems;
    }

    /// <summary>
    /// Mapper for ApplicationSurvey
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static ApplicationSurveyItem Create(ApplicationSurvey item)
    {
        return new ApplicationSurveyItem
        {
            ApplicationSurveyID = item.ApplicationSurveyId,
            ApplicationID = item.ApplicationId,
            Survey = Create(item.Survey) ?? new SurveyItem(),
        };
    }
    /// <summary>
    /// Mapper for Survey
    /// </summary>
    /// <param name="survey"></param>
    /// <returns></returns>
    public static SurveyItem Create(Survey survey)
    {
        if (survey == null) return new();
        return new SurveyItem
        {
            SurveyID = survey.SurveyId,
            SurveyNM = survey.SurveyNm,
            SurveyDS = survey.SurveyDs ?? string.Empty,
            StartDT = survey.StartDt,
            EndDT = survey.EndDt,
            StatusList = Create(survey.SurveyStatuses),
            QuestionGroupList = Create(survey.QuestionGroups),
        };
    }

    /// <summary>
    /// Mapper for SurveyStatus
    /// </summary>
    /// <param name="surveyStatuses"></param>
    /// <returns></returns>
    public static List<SurveyStatusItem> Create(ICollection<SurveyStatus>? surveyStatuses)
    {
        List<SurveyStatusItem> surveyStatusItems = [];
        if (surveyStatuses == null) return surveyStatusItems;

        foreach (var item in surveyStatuses)
        {
            surveyStatusItems.Add(Create(item));
        }
        return surveyStatusItems;
    }
    /// <summary>
    /// Mapper for SurveyStatus
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static SurveyStatusItem Create(SurveyStatus item)
    {
        return new SurveyStatusItem
        {
            SurveyStatusID = item.SurveyStatusId,
            SurveyID = item.SurveyId,
            StatusID = item.StatusId,
            StatusNM = item.StatusNm,
            StatusDS = item.StatusDs ?? string.Empty,
            SubjectTemplate = item.EmailSubjectTemplate ?? string.Empty,
            BodyTemplate = item.EmailTemplate ?? string.Empty,
            ModifiedID = item.ModifiedId,
            NextStatusID = item.NextStatusId,
            PreviousStatusID = item.PreviousStatusId,
        };
    }

    /// <summary>
    /// Mapper for Company List
    /// </summary>
    /// <param name="company"></param>
    /// <returns></returns>
    public static CompanyItem[] Create(Company[] company)
    {
        int companyCount = company.Length;
        CompanyItem[] companyItemArray = new CompanyItem[companyCount];

        for (int i = 0; i < companyCount; i++)
        {
            var item = company[i];
            companyItemArray[i] = Create(item);
        }
        return companyItemArray;
    }

    /// <summary>
    /// Mapper for Company Item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static CompanyItem Create(Company item)
    {
        return new CompanyItem
        {
            CompanyID = item.CompanyId,
            CompanyNM = item.CompanyNm,
            CompanyCD = item.CompanyCd,
            CompanyDS = item.CompanyDs ?? item.CompanyNm,
            Title = item.Title,
            SiteTheme = item.Theme,
            DefaultSiteTheme = item.DefaultTheme,
            GalleryFolder = item.GalleryFolder,
            Address1 = item.Address1,
            Address2 = item.Address2 ?? string.Empty,
            City = item.City,
            State = item.State,
            Country = item.Country,
            PostalCode = item.PostalCode,
            SiteURL = item.SiteUrl,
            FromEmail = item.FromEmail ?? string.Empty,
            SMTP = item.Smtp ?? string.Empty,
            Component = item.Component ?? string.Empty,
            ModifiedID = item.ModifiedId,
            ModifiedDT = item.ModifiedDt,
            Active = item.ActiveFl,
            ProjectCount = item.Applications?.Count ?? 0,
            UserCount = item.ApplicationUsers?.Count ?? 0,
            SurveyResponseCount = 0,
            UserList = Create(item.ApplicationUsers),
            ProjectList = Create(item.Applications)
        };
    }

    /// <summary>
    /// Mapper for Application List
    /// </summary>
    /// <param name="applications"></param>
    /// <returns></returns>
    public static List<ApplicationItem> Create(ICollection<Application>? applications)
    {
        List<ApplicationItem> applicationItems = [];
        if (applications == null) return applicationItems;

        foreach (var item in applications)
        {
            applicationItems.Add(Create(item));
        }
        return applicationItems;
    }

    /// <summary>
    /// Mapper for ApplicationUser List
    /// </summary>
    /// <param name="applicationUsers"></param>
    /// <returns></returns>
    public static List<ApplicationUserItem> Create(ICollection<ApplicationUser>? applicationUsers)
    {
        List<ApplicationUserItem> applicationUserItems = [];

        if (applicationUsers == null) return applicationUserItems;

        foreach (var item in applicationUsers)
        {
            applicationUserItems.Add(new ApplicationUserItem
            {
                ApplicationUserID = item.ApplicationUserId,
                AccountNM = item.AccountNm,
                CompanyID = item.CompanyId ?? 0,
                ModifiedID = item.ModifiedId,
                ModifiedDT = item.ModifiedDt,
            });
        }
        return applicationUserItems;

    }

    /// <summary>
    /// Mapper for ApplicationType List
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static ApplicationTypeItem Create(LuApplicationType s)
    {
        return new ApplicationTypeItem()
        {
            ApplicationTypeID = s.ApplicationTypeId,
            ApplicationTypeNM = s.ApplicationTypeNm,
            ApplicationTypeDS = s.ApplicationTypeDs ?? string.Empty,
            ModifiedID = s.ModifiedId,
            ModifiedDT = s.ModifiedDt,
        };
    }
    /// <summary>
    /// Mapper for SurveyType List
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static SurveyTypeItem Create(LuSurveyType s)
    {
        return new SurveyTypeItem()
        {
            SurveyTypeID = s.SurveyTypeId,
            SurveyTypeShortNM = s.SurveyTypeShortNm,
            SurveyTypeNM = s.SurveyTypeNm,
            SurveyTypeDS = s.SurveyTypeDs ?? string.Empty,
            SurveyTypeComment = s.SurveyTypeComment ?? string.Empty,
            ApplicationTypeID = s.ApplicationTypeId,
            MultiSequence = s.MutiSequenceFl,
            ParentSurveyTypeID = s.ParentSurveyTypeId ?? 0,
            ParentSurveyTypeNM = s.SurveyTypeNm,
            LevelNumber = 0,
            TreeSort = string.Empty,
            ModifiedID = s.ModifiedId,
            ModifiedDT = s.ModifiedDt,
            QuestionCount = s.Questions?.Count ?? 0,
            SurveyCount = s.Surveys?.Count ?? 0,
            ChildCount = 0,
        };
    }

    /// <summary>
    /// Mapper for Question List
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static ApplicationUserItem Create(ApplicationUser s)
    {
        return new ApplicationUserItem()
        {
            ApplicationUserID = s.ApplicationUserId,
            AccountNM = s.AccountNm,
            FirstNM = s.FirstNm,
            LastNM = s.LastNm,
            EMailAddress = s.EMailAddress,
            CommentDS = s.CommentDs ?? string.Empty,
            CompanyID = s.CompanyId ?? 0,
            CompanyNM = s.Company?.CompanyNm ?? string.Empty,
            SupervisorAccountNM = s.SupervisorAccountNm ?? string.Empty,
            LastLoginDT = s.LastLoginDt ?? DateTime.MinValue,
            LastLoginLocation = s.LastLoginLocation ?? string.Empty,
            ModifiedID = s.ModifiedId,
            ModifiedDT = s.ModifiedDt,
            UserRoleID = s.RoleId
        };
    }
}
