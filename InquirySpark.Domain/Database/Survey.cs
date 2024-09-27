namespace InquirySpark.Domain.Database;

public partial class Survey
{
    /// <summary>
    /// Gets or sets the survey ID.
    /// </summary>
    [DisplayName("Survey ID")]
    [Key]
    public int SurveyId { get; set; }

    /// <summary>
    /// Gets or sets the survey type ID.
    /// </summary>
    [DisplayName("Survey Type")]
    public int SurveyTypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use question groups.
    /// </summary>
    [DisplayName("Use Question Groups")]
    public bool UseQuestionGroupsFl { get; set; }

    /// <summary>
    /// Gets or sets the survey name.
    /// </summary>
    [DisplayName("Survey CompanyNm")]
    public string SurveyNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the survey short name.
    /// </summary>
    [DisplayName("Survey Short CompanyNm")]
    public string SurveyShortNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the survey description.
    /// </summary>
    [DisplayName("Survey Description")]
    public string SurveyDs { get; set; } = null!;

    /// <summary>
    /// Gets or sets the completion message.
    /// </summary>
    [DisplayName("Completion Message")]
    public string CompletionMessage { get; set; } = null!;

    /// <summary>
    /// Gets or sets the response name template.
    /// </summary>
    [DisplayName("Response CompanyNm Template")]
    public string? ResponseNmtemplate { get; set; }

    /// <summary>
    /// Gets or sets the reviewer account name.
    /// </summary>
    [DisplayName("Reviewer Account CompanyNm")]
    public string? ReviewerAccountNm { get; set; }

    /// <summary>
    /// Gets or sets the auto assign filter.
    /// </summary>
    [DisplayName("Auto Assign Filter")]
    public string? AutoAssignFilter { get; set; }

    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    [DisplayName("Start Date")]
    public DateTime? StartDt { get; set; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    [DisplayName("End Date")]
    public DateTime? EndDt { get; set; }

    /// <summary>
    /// Gets or sets the parent survey ID.
    /// </summary>
    [DisplayName("Parent Survey ID")]
    public int? ParentSurveyId { get; set; }

    /// <summary>
    /// Gets or sets the modified ID.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the modified date.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the application surveys.
    /// </summary>
    [DisplayName("Application Surveys")]
    public virtual ICollection<ApplicationSurvey> ApplicationSurveys { get; set; } = [];

    /// <summary>
    /// Gets or sets the question groups.
    /// </summary>
    [DisplayName("Question Groups")]
    public virtual ICollection<QuestionGroup> QuestionGroups { get; set; } = [];

    /// <summary>
    /// Gets or sets the survey email templates.
    /// </summary>
    [DisplayName("Survey Email Templates")]
    public virtual ICollection<SurveyEmailTemplate> SurveyEmailTemplates { get; set; } = [];

    /// <summary>
    /// Gets or sets the survey responses.
    /// </summary>
    [DisplayName("Survey Responses")]
    public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = [];

    /// <summary>
    /// Gets or sets the survey review statuses.
    /// </summary>
    [DisplayName("Survey Review Statuses")]
    public virtual ICollection<SurveyReviewStatus> SurveyReviewStatuses { get; set; } = [];

    /// <summary>
    /// Gets or sets the survey statuses.
    /// </summary>
    [DisplayName("Survey Statuses")]
    public virtual ICollection<SurveyStatus> SurveyStatuses { get; set; } = [];

    /// <summary>
    /// Gets or sets the survey type.
    /// </summary>
    [DisplayName("Survey Type")]
    public virtual LuSurveyType SurveyType { get; set; } = null!;
}
