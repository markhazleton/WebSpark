
namespace ControlSpark.Domain.Models;

/// <summary>
/// Class LinkModel.
/// </summary>
public class LinkModel
{
    /// <summary>
    /// Gets or sets the href.
    /// </summary>
    /// <value>The href.</value>
    public string Href { get; set; }

    /// <summary>
    /// Gets or sets the relative.
    /// </summary>
    /// <value>The relative.</value>
    public string Rel { get; set; }

    /// <summary>
    /// Gets or sets the method.
    /// </summary>
    /// <value>The method.</value>
    public string Method { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is templated.
    /// </summary>
    /// <value><c>true</c> if this instance is templated; otherwise, <c>false</c>.</value>
    public bool IsTemplated { get; set; }
}
