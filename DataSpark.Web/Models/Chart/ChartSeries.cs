using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataSpark.Web.Models.Chart;

/// <summary>
/// Represents a data series within a chart configuration
/// </summary>
public class ChartSeries
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("ChartConfiguration")]
    public int ChartConfigurationId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DataColumn { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string AggregationFunction { get; set; } = "Sum";

    [StringLength(50)]
    public string SeriesChartType { get; set; } = string.Empty; // Empty means use chart default

    [StringLength(20)]
    public string Color { get; set; } = string.Empty; // Hex color code

    public bool IsVisible { get; set; } = true;

    public int DisplayOrder { get; set; }

    [StringLength(20)]
    public string LineStyle { get; set; } = "Solid"; // Solid, Dash, Dot, DashDot

    public int LineWidth { get; set; } = 2;

    [StringLength(20)]
    public string MarkerStyle { get; set; } = "None"; // None, Circle, Square, Diamond, Triangle

    public int MarkerSize { get; set; } = 6;

    public bool ShowDataLabels { get; set; } = false;

    [StringLength(20)]
    public string DataLabelPosition { get; set; } = "Top"; // Top, Bottom, Left, Right, Center

    // For dual-axis support
    [StringLength(10)]
    public string YAxisType { get; set; } = "Primary"; // Primary, Secondary

    // Navigation property
    public ChartConfiguration? ChartConfiguration { get; set; }

    /// <summary>
    /// Gets the effective chart type for this series
    /// </summary>
    public string GetEffectiveChartType()
    {
        return string.IsNullOrWhiteSpace(SeriesChartType)
            ? ChartConfiguration?.ChartType ?? "Column"
            : SeriesChartType;
    }

    /// <summary>
    /// Validates the series configuration
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Series name is required");

        if (string.IsNullOrWhiteSpace(DataColumn))
            errors.Add("Data column is required");

        if (string.IsNullOrWhiteSpace(AggregationFunction))
            errors.Add("Aggregation function is required");

        var validAggregations = new[] { "Sum", "Average", "Count", "Min", "Max", "Median", "StdDev" };
        if (!validAggregations.Contains(AggregationFunction))
            errors.Add($"Invalid aggregation function. Valid options: {string.Join(", ", validAggregations)}");

        if (DisplayOrder < 0)
            errors.Add("Display order must be non-negative");

        return errors;
    }
}

/// <summary>
/// Available aggregation functions for chart series
/// </summary>
public static class AggregationFunctions
{
    public const string Sum = "Sum";
    public const string Average = "Average";
    public const string Count = "Count";
    public const string Min = "Min";
    public const string Max = "Max";
    public const string Median = "Median";
    public const string StandardDeviation = "StdDev";

    public static readonly string[] All = { Sum, Average, Count, Min, Max, Median, StandardDeviation };

    public static readonly Dictionary<string, string> Descriptions = new()
    {
        { Sum, "Sum of all values" },
        { Average, "Average (mean) of all values" },
        { Count, "Count of data points" },
        { Min, "Minimum value" },
        { Max, "Maximum value" },
        { Median, "Median value" },
        { StandardDeviation, "Standard deviation" }
    };
}
