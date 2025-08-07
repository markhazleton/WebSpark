using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataSpark.Web.Models.Chart;

/// <summary>
/// Represents a complete chart configuration including all settings, series, axes, and filters
/// </summary>
public class ChartConfiguration
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string CsvFile { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ChartType { get; set; } = "Column";

    [StringLength(10)]
    public string ChartStyle { get; set; } = "2D"; // 2D, 3D

    [StringLength(50)]
    public string ChartPalette { get; set; } = "BrightPastel";

    public int Width { get; set; } = 800;

    public int Height { get; set; } = 400;

    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(200)]
    public string SubTitle { get; set; } = string.Empty;

    [StringLength(255)]
    public string BackgroundImage { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation Properties
    public List<ChartSeries> Series { get; set; } = new();

    public ChartAxis? XAxis { get; set; }

    public ChartAxis? YAxis { get; set; }

    public ChartAxis? Y2Axis { get; set; }

    public List<ChartFilter> Filters { get; set; } = new();

    // Additional properties for advanced configuration
    public bool ShowLegend { get; set; } = true;

    [StringLength(20)]
    public string LegendPosition { get; set; } = "Right";

    public bool ShowTooltips { get; set; } = true;

    public bool EnableZoom { get; set; } = true;

    public bool EnablePan { get; set; } = true;

    public bool IsAnimated { get; set; } = true;

    public int AnimationDuration { get; set; } = 1000;

    [StringLength(20)]
    public string Theme { get; set; } = "Default";

    // Chart-specific options stored as JSON
    public string? ChartOptions { get; set; }

    /// <summary>
    /// Validates the chart configuration
    /// </summary>
    public ValidationResult Validate()
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(Name))
            result.Errors.Add("Chart name is required");

        if (string.IsNullOrWhiteSpace(CsvFile))
            result.Errors.Add("CSV file is required");

        if (Series == null || Series.Count == 0)
            result.Errors.Add("At least one data series is required");

        if (Width <= 0 || Width > 2000)
            result.Errors.Add("Width must be between 1 and 2000 pixels");

        if (Height <= 0 || Height > 2000)
            result.Errors.Add("Height must be between 1 and 2000 pixels");

        return result;
    }

    /// <summary>
    /// Creates a deep copy of the configuration
    /// </summary>
    public ChartConfiguration Clone()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(this);
        var clone = System.Text.Json.JsonSerializer.Deserialize<ChartConfiguration>(json)!;
        clone.Id = 0; // Reset ID for new configuration
        clone.CreatedDate = DateTime.UtcNow;
        clone.ModifiedDate = DateTime.UtcNow;
        return clone;
    }
}

/// <summary>
/// Represents validation results for chart configuration
/// </summary>
public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
