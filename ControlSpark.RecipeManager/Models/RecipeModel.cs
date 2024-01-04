namespace ControlSpark.RecipeManager.Models;

/// <summary>
/// Class RecipeModel.
/// </summary>
public class RecipeModel
{
    /// <summary>
    /// 
    /// </summary>
    public RecipeModel()
    {
        RecipeCategory = new RecipeCategoryModel();
    }
    /// <summary>
    /// Gets or sets the author nm.
    /// </summary>
    /// <value>The author nm.</value>
    [DisplayName("Author")]
    [Required]
    public string AuthorNM { get; set; }

    /// <summary>
    /// Gets or sets the average rating.
    /// </summary>
    /// <value>The average rating.</value>
    [DisplayName("Average Ratings")]
    public double AverageRating { get; set; }

    /// <summary>
    /// Gets or sets the comment count.
    /// </summary>
    /// <value>The comment count.</value>
    [DisplayName("Comments")]
    public int CommentCount { get; set; }

    /// <summary>
    /// Gets or sets the file description.
    /// </summary>
    /// <value>The file description.</value>
    public string FileDescription { get; set; }

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the ingredient ds.
    /// </summary>
    /// <value>The ingredient ds.</value>
    [DisplayName("Ingredients")]
    public string Ingredients { get; set; }

    /// <summary>
    /// Gets or sets the instruction ds.
    /// </summary>
    /// <value>The instruction ds.</value>
    [DisplayName("Instructions")]
    public string Instructions { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is approved.
    /// </summary>
    /// <value><c>true</c> if this instance is approved; otherwise, <c>false</c>.</value>
    [DisplayName("Approved")]
    public bool IsApproved { get; set; }

    /// <summary>
    /// Gets or sets the last view dt.
    /// </summary>
    /// <value>The last view dt.</value>
    [DisplayName("Last View")]
    public DateTime LastViewDT { get; set; }

    /// <summary>
    /// Gets or sets the modified dt.
    /// </summary>
    /// <value>The modified dt.</value>
    [DisplayName("Last Modified")]
    public DateTime ModifiedDT { get; set; }

    /// <summary>
    /// Gets or sets the modified identifier.
    /// </summary>
    /// <value>The modified identifier.</value>
    public int ModifiedID { get; set; }

    /// <summary>
    /// Gets or sets the rating count.
    /// </summary>
    /// <value>The rating count.</value>
    [DisplayName("Ratings Count")]
    public int RatingCount { get; set; }

    /// <summary>
    /// The number of servings this recipe makes
    /// </summary>
    [DisplayName("Servings")]
    public int Servings { get; set; }

    /// <summary>
    /// RecipeCategory
    /// </summary>
    public RecipeCategoryModel RecipeCategory { get; set; }
    /// <summary>
    /// Gets or sets the recipe category identifier.
    /// </summary>
    /// <value>The recipe category identifier.</value>
    [DisplayName("Category")]
    [Required]
    public int RecipeCategoryID { get; set; }

    /// <summary>
    /// Gets or sets the recipe category nm.
    /// </summary>
    /// <value>The recipe category nm.</value>
    [DisplayName("Category")]
    public string RecipeCategoryNM { get; set; }

    /// <summary>
    /// Gets or sets the recipe ds.
    /// </summary>
    /// <value>The recipe ds.</value>
    [DisplayName("Description")]
    [Required]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the recipe identifier.
    /// </summary>
    /// <value>The recipe identifier.</value>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the recipe nm.
    /// </summary>
    /// <value>The recipe nm.</value>
    [DisplayName("Recipe")]
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the view count.
    /// </summary>
    /// <value>The view count.</value>
    [DisplayName("View Count")]
    public int ViewCount { get; set; }

    /// <summary>
    /// Gets or sets the recipe URL.
    /// </summary>
    /// <value>The recipe URL.</value>
    public string RecipeURL { get; set; }

    public List<RecipeImageModel> Images { get; set; } = new List<RecipeImageModel>();

    /// <summary>
    /// Lookup List of Recipe Categories
    /// </summary>
    public IEnumerable<LookupModel> RecipeCategories { get; set; }
    public int DomainID { get; set; } = 1;
}
