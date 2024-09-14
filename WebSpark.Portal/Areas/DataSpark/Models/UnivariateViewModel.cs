namespace WebSpark.Portal.Areas.DataSpark.Models;

/// <summary>
/// Represents a view model for the Univariate analysis.
/// </summary>
public class UnivariateViewModel
{
    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the list of available columns.
    /// </summary>
    public List<string> AvailableColumns { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the selected column.
    /// </summary>
    public string SelectedColumn { get; set; }

    /// <summary>
    /// Gets or sets the image path.
    /// </summary>
    public string ImagePath { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Message { get; set; }
}

