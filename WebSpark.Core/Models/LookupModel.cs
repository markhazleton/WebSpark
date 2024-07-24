namespace WebSpark.Core.Models;

/// <summary>
/// Class LookupModel.
/// </summary>
public class LookupModel
{
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <value>The text.</value>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is default.
    /// </summary>
    /// <value><c>true</c> if this instance is default; otherwise, <c>false</c>.</value>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is selected.
    /// </summary>
    /// <value><c>true</c> if this instance is selected; otherwise, <c>false</c>.</value>
    public bool IsSelected { get; set; }
}
