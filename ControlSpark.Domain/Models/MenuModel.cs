
namespace ControlSpark.Domain.Models;


/// <summary>
/// Class MenuModel.
/// </summary>
public class MenuModel
{
    private DateTime? _lastModified;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuModel" /> class.
    /// </summary>
    public MenuModel()
    {
        DisplayOrder = 1;
        DisplayInNavigation = true;
    }

    /// <summary>
    /// Gets or sets the action.
    /// </summary>
    /// <value>The action.</value>
    [JsonPropertyName("action")]
    public string Action { get; set; }
    /// <summary>
    /// Gets or sets the Api Url.
    /// </summary>
    /// <value>The URL.</value>
    [DisplayName("Api Url")]
    [JsonPropertyName("api_url")]
    public string ApiUrl { get; set; }
    /// <summary>
    /// Gets or sets the Argument for the controller action.
    /// </summary>
    /// <value>The Argument.</value>
    [JsonPropertyName("argument")]
    public string? Argument { get; set; }

    /// <summary>
    /// Gets or sets the controller.
    /// </summary>
    /// <value>The controller.</value>
    [JsonPropertyName("controller")]
    public string Controller { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    [JsonPropertyName("description")]
    [DisplayName("Description")]
    [StringLength(100, ErrorMessage = "Max length is 100.")]
    [Required]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [display in navigation].
    /// </summary>
    /// <value><c>true</c> if [display in navigation]; otherwise, <c>false</c>.</value>
    [JsonPropertyName("display_navigation")]
    public bool DisplayInNavigation { get; set; }

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    /// <value>The display order.</value>
    [JsonPropertyName("order")]
    [DisplayName("Display Order")]
    [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please Enter Valid Number")]
    [Required]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the domain identifier.
    /// </summary>
    /// <value>The domain identifier.</value>
    [DisplayName("Website Domain")]
    [JsonPropertyName("domain_id")]
    public int DomainID { get; set; }

    /// <summary>
    /// Domain Name
    /// </summary>
    [JsonPropertyName("domain_name")]
    public string DomainName { get; set; }
    /// <summary>
    /// Domain Url for API
    /// </summary>
    [JsonPropertyName("domain_url")]
    public string DomainUrl { get; set; }
    /// <summary>
    /// Gets or sets the icon.
    /// </summary>
    /// <value>The icon.</value>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    [JsonPropertyName("id")]
    public int Id { get; set; }
    public bool IsHomePage { get; set; } = false;

    /// <summary>
    /// Gets or sets the last modified.
    /// </summary>
    /// <value>The last modified.</value>
    [JsonPropertyName("modified")]
    public DateTime? LastModified
    {
        get { return _lastModified; }
        set { _lastModified = value; }
    }

    /// <summary>
    /// Gets or sets the last modified in W3C Datetime format.
    /// </summary>
    /// <value>The last modified in W3C Datetime format.</value>
    [JsonPropertyName("modified_w3c")]
    public string LastModifiedW3C
    {
        get
        {
            if (_lastModified == null)
            {
                _lastModified = DateTime.Now;
            }

            return _lastModified.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
        set { _lastModified = DateTime.TryParse(value, out var result) ? result : (DateTime?)null; }
    }





    /// <summary>
    /// Gets or sets the content of the page.
    /// </summary>
    /// <value>The content of the page.</value>
    [JsonPropertyName("content")]
    [DisplayName("Page Content")]
    public string? PageContent { get; set; }

    /// <summary>
    /// Gets or sets the parent controller.
    /// </summary>
    /// <value>The parent controller.</value>
    [JsonPropertyName("parent")]
    public string ParentController { get; set; }

    /// <summary>
    /// Gets or sets the parent identifier.
    /// </summary>
    /// <value>The parent identifier.</value>
    [JsonPropertyName("parent_page")]
    [DisplayName("Parent Page")]
    public int? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the parent title.
    /// </summary>
    /// <value>The parent title.</value>
    [JsonPropertyName("parent_title")]
    public string ParentTitle { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>The title.</value>
    [JsonPropertyName("title")]
    [DisplayName("Title")]
    [StringLength(100, ErrorMessage = "Max length is 100.")]
    [Required]
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the URL.
    /// </summary>
    /// <value>The URL.</value>
    [DisplayName("Page Url")]
    [StringLength(100, ErrorMessage = "Max length is 100.")]
    [Required]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the virtual path.
    /// </summary>
    /// <value>The virtual path.</value>
    [JsonPropertyName("virtual_path")]
    public string VirtualPath { get; set; }
}
