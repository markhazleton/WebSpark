namespace WebSpark.Core.Models;

/// <summary>
/// Class ListModel.
/// </summary>
public class LookupListModel
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>The title.</value>
    public string Title { get; set; }

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <value>The items.</value>
    public List<LookupListItemModel> Items { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListModel" /> class.
    /// </summary>
    public LookupListModel()
    {
        Items = [];
    }
}
