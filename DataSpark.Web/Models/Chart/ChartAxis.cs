using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DataSpark.Web.Models.Chart;

/// <summary>
/// Represents axis configuration for charts
/// </summary>
public class ChartAxis
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("ChartConfiguration")]
    public int ChartConfigurationId { get; set; }

    [Required]
    [StringLength(10)]
    public string AxisType { get; set; } = "X"; // X, Y, Y2

    [StringLength(100)]
    public string DataColumn { get; set; } = string.Empty;

    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public double? MinValue { get; set; }

    public double? MaxValue { get; set; }

    public double? Interval { get; set; }

    public bool IsLogarithmic { get; set; } = false;

    public double LogarithmBase { get; set; } = 10;

    [StringLength(20)]
    public string ScaleType { get; set; } = "Linear"; // Linear, Logarithmic, DateTime

    // Serialized as JSON for flexibility
    public string? SelectedValuesJson { get; set; }

    // Grid line configuration
    public bool ShowGridLines { get; set; } = true;

    [StringLength(20)]
    public string GridLineColor { get; set; } = "#E0E0E0";

    [StringLength(20)]
    public string GridLineStyle { get; set; } = "Solid"; // Solid, Dash, Dot

    public int GridLineWidth { get; set; } = 1;

    // Tick mark configuration
    public bool ShowTickMarks { get; set; } = true;

    [StringLength(20)]
    public string TickMarkColor { get; set; } = "#808080";

    public int TickMarkLength { get; set; } = 3;

    // Label configuration
    public bool ShowLabels { get; set; } = true;

    [StringLength(50)]
    public string LabelFont { get; set; } = "Arial";

    public int LabelFontSize { get; set; } = 10;

    [StringLength(20)]
    public string LabelColor { get; set; } = "#000000";

    public double LabelAngle { get; set; } = 0;

    public bool AutoLabelAngle { get; set; } = true;

    // Title configuration
    [StringLength(50)]
    public string TitleFont { get; set; } = "Arial";

    public int TitleFontSize { get; set; } = 12;

    [StringLength(20)]
    public string TitleColor { get; set; } = "#000000";

    // Date/time specific settings
    [StringLength(50)]
    public string DateTimeFormat { get; set; } = "MM/dd/yyyy";

    [StringLength(20)]
    public string DateTimeIntervalType { get; set; } = "Auto"; // Auto, Days, Weeks, Months, Years

    // Navigation property
    public ChartConfiguration? ChartConfiguration { get; set; }

    /// <summary>
    /// Gets the selected values as a list of strings
    /// </summary>
    [NotMapped]
    public List<string> SelectedValues
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SelectedValuesJson))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(SelectedValuesJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
        set
        {
            SelectedValuesJson = JsonSerializer.Serialize(value);
        }
    }

    /// <summary>
    /// Validates the axis configuration
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        var validAxisTypes = new[] { "X", "Y", "Y2" };
        if (!validAxisTypes.Contains(AxisType))
            errors.Add($"Invalid axis type. Valid options: {string.Join(", ", validAxisTypes)}");

        if (MinValue.HasValue && MaxValue.HasValue && MinValue >= MaxValue)
            errors.Add("Minimum value must be less than maximum value");

        if (Interval.HasValue && Interval <= 0)
            errors.Add("Interval must be greater than zero");

        if (IsLogarithmic && LogarithmBase <= 1)
            errors.Add("Logarithm base must be greater than 1");

        var validScaleTypes = new[] { "Linear", "Logarithmic", "DateTime" };
        if (!validScaleTypes.Contains(ScaleType))
            errors.Add($"Invalid scale type. Valid options: {string.Join(", ", validScaleTypes)}");

        if (LabelFontSize <= 0 || LabelFontSize > 72)
            errors.Add("Label font size must be between 1 and 72");

        if (TitleFontSize <= 0 || TitleFontSize > 72)
            errors.Add("Title font size must be between 1 and 72");

        if (Math.Abs(LabelAngle) > 90)
            errors.Add("Label angle must be between -90 and 90 degrees");

        return errors;
    }

    /// <summary>
    /// Gets the effective minimum value considering data and user settings
    /// </summary>
    public double GetEffectiveMinValue(double dataMin)
    {
        if (MinValue.HasValue)
            return MinValue.Value;

        // Auto-calculate based on data and scale type
        if (IsLogarithmic && dataMin <= 0)
            return 1; // Can't have zero or negative values on log scale

        return dataMin < 0 ? dataMin * 1.1 : dataMin * 0.9;
    }

    /// <summary>
    /// Gets the effective maximum value considering data and user settings
    /// </summary>
    public double GetEffectiveMaxValue(double dataMax)
    {
        if (MaxValue.HasValue)
            return MaxValue.Value;

        // Auto-calculate based on data
        return dataMax > 0 ? dataMax * 1.1 : dataMax * 0.9;
    }
}
