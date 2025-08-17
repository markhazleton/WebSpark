namespace WebSpark.Core.Models;

/// <summary>
/// Class RecipeCategoryModel.
/// </summary>
public class RecipeCategoryModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeCategoryModel" /> class.
    /// </summary>
    public RecipeCategoryModel()
    {
        Recipes = [];
    }

    /// <summary>
    /// Gets or sets the recipe category identifier.
    /// </summary>
    /// <value>The recipe category identifier.</value>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the recipe category name.
    /// </summary>
    /// <value>The recipe category nm.</value>
    [JsonPropertyName("name")]
    [DisplayName("Category")]
    [StringLength(50, ErrorMessage = "Max length is 50.")]
    [DataType(DataType.Text)]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipe category Description.
    /// </summary>
    /// <value>The recipe category cm.</value>
    [JsonPropertyName("description")]
    [DisplayName("Description")]
    [StringLength(100, ErrorMessage = "Max length is 100.")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    /// <value>The display order.</value>
    [JsonPropertyName("order")]
    [DisplayName("Order")]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is active.
    /// </summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the recipes.
    /// </summary>
    /// <value>The recipes.</value>
    public List<RecipeModel> Recipes { get; set; }
    /// <summary>
    /// Link to API
    /// </summary>
    public string Url { get; set; } = string.Empty;
    public int DomainID { get; set; } = RecipeConstants.INT_MOM_DomainId;

}
