namespace InquirySpark.Domain.Database;
/// <summary>
/// Company entity.
/// </summary>
[DisplayColumn("CompanyNm")]
public partial class Company
{
    /// <summary>
    /// Gets or sets the company ID.
    /// </summary>
    [Display(Name = "ID")]
    [Key]
    public int CompanyId { get; set; }

    /// <summary>
    /// Gets or sets the company name.
    /// </summary>
    [Display(Name = "CompanyNm")]
    [Column(name: "CompanyNM", Order = 1)]
    public string CompanyNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the company code.
    /// </summary>
    [Display(Name = "Code")]
    public string CompanyCd { get; set; } = null!;

    /// <summary>
    /// Gets or sets the company description.
    /// </summary>
    [Display(Name = "Description")]
    public string? CompanyDs { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [Display(Name = "Title")]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets the theme.
    /// </summary>
    [Display(Name = "Theme")]
    public string Theme { get; set; } = null!;

    /// <summary>
    /// Gets or sets the default theme.
    /// </summary>
    [Display(Name = "Default Theme")]
    public string DefaultTheme { get; set; } = null!;

    /// <summary>
    /// Gets or sets the gallery folder.
    /// </summary>
    [Display(Name = "Gallery Folder")]
    public string GalleryFolder { get; set; } = null!;

    /// <summary>
    /// Gets or sets the site URL.
    /// </summary>
    [Display(Name = "Site URL")]
    public string SiteUrl { get; set; } = null!;

    /// <summary>
    /// Gets or sets the address line 1.
    /// </summary>
    [Display(Name = "Address Line 1")]
    public string Address1 { get; set; } = null!;

    /// <summary>
    /// Gets or sets the address line 2.
    /// </summary>
    [Display(Name = "Address Line 2")]
    public string? Address2 { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    [Display(Name = "City")]
    public string City { get; set; } = null!;

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    [Display(Name = "State")]
    public string State { get; set; } = null!;

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    [Display(Name = "Country")]
    public string Country { get; set; } = null!;

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = null!;

    /// <summary>
    /// Gets or sets the fax number.
    /// </summary>
    [Display(Name = "Fax Number")]
    public string? FaxNumber { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the default payment terms.
    /// </summary>
    [Display(Name = "Default Payment Terms")]
    public string? DefaultPaymentTerms { get; set; }

    /// <summary>
    /// Gets or sets the default invoice description.
    /// </summary>
    [Display(Name = "Default Invoice Description")]
    public string? DefaultInvoiceDescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the company is active.
    /// </summary>
    [Display(Name = "Active")]
    public bool ActiveFl { get; set; }

    /// <summary>
    /// Gets or sets the component.
    /// </summary>
    [Display(Name = "Component")]
    public string? Component { get; set; }

    /// <summary>
    /// Gets or sets the from email.
    /// </summary>
    [Display(Name = "From Email")]
    public string? FromEmail { get; set; }

    /// <summary>
    /// Gets or sets the SMTP server.
    /// </summary>
    [Display(Name = "SMTP Server")]
    public string? Smtp { get; set; }

    /// <summary>
    /// Gets or sets the modified date.
    /// </summary>
    [Display(Name = "Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the modified ID.
    /// </summary>
    [Display(Name = "Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the collection of application users.
    /// </summary>
    public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; } = new List<ApplicationUser>();

    /// <summary>
    /// Gets or sets the collection of applications.
    /// </summary>
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
}
