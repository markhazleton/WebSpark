using System;

namespace InquirySpark.Domain.Database;

public partial class SurveyStatus
{
    public int SurveyStatusId { get; set; }

    public int SurveyId { get; set; }

    public int StatusId { get; set; }

    public string StatusNm { get; set; } = null!;

    public string StatusDs { get; set; } = null!;

    public string EmailTemplate { get; set; } = null!;

    public string EmailSubjectTemplate { get; set; } = null!;

    public int PreviousStatusId { get; set; }

    public int NextStatusId { get; set; }

    public int ModifiedId { get; set; }

    public DateTime ModifiedDt { get; set; }

    public virtual Survey Survey { get; set; } = null!;
}
