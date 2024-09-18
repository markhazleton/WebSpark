using System;

namespace InquirySpark.Domain.Database;

public partial class WebPortal
{
    public int WebPortalId { get; set; }

    public string WebPortalNm { get; set; } = null!;

    public string? WebPortalDs { get; set; }

    public string WebPortalUrl { get; set; } = null!;

    public string WebServiceUrl { get; set; } = null!;

    public bool? ActiveFl { get; set; }

    public int ModifiedId { get; set; }

    public DateTime ModifiedDt { get; set; }
}
