namespace DataSpark.Web.Models;

public class ColumnInfo
{
    // Existing properties
    public bool IsNumeric { get; set; }
    public bool IsCategory { get; set; }
    public string? Column { get; set; }
    public string? Type { get; set; }
    public long NonNullCount { get; set; }
    public long NullCount { get; set; }
    public long UniqueCount { get; set; }
    public object? MostCommonValue { get; set; }
    public double Skewness { get; set; }
    public object? Min { get; set; }
    public object? Max { get; set; }
    public double Mean { get; set; }
    public double StandardDeviation { get; set; }
    public List<string> Errors { get; set; } = []; // New property for capturing errors
    public List<string> Observations { get; set; } = []; // New property for storing detailed observations

    // Enhanced properties for improved analysis

    /// <summary>
    /// Mode value for the column (most frequently occurring value)
    /// </summary>
    public object? Mode { get; set; }

    /// <summary>
    /// Frequency count of the mode value
    /// </summary>
    public long ModeFrequency { get; set; }

    /// <summary>
    /// Median value for numeric columns
    /// </summary>
    public double? Median { get; set; }

    /// <summary>
    /// First quartile (25th percentile) for numeric columns
    /// </summary>
    public double? Q1 { get; set; }

    /// <summary>
    /// Third quartile (75th percentile) for numeric columns
    /// </summary>
    public double? Q3 { get; set; }

    /// <summary>
    /// Interquartile range (Q3 - Q1) for numeric columns
    /// </summary>
    public double? IQR { get; set; }

    /// <summary>
    /// Coefficient of variation (Standard Deviation / Mean * 100)
    /// </summary>
    public double? CoefficientOfVariation { get; set; }

    /// <summary>
    /// Number of outliers detected using standard deviation method
    /// </summary>
    public int OutlierCountStdDev { get; set; }

    /// <summary>
    /// Number of outliers detected using IQR method
    /// </summary>
    public int OutlierCountIQR { get; set; }

    /// <summary>
    /// Percentage of missing values
    /// </summary>
    public double MissingPercentage { get; set; }

    /// <summary>
    /// Percentage of duplicate values
    /// </summary>
    public double DuplicatePercentage { get; set; }

    /// <summary>
    /// Data quality score (0-100) based on completeness and other factors
    /// </summary>
    public double DataQualityScore { get; set; }

    /// <summary>
    /// Information content score (0-100) based on uniqueness and distribution
    /// </summary>
    public double InformationContentScore { get; set; }

    /// <summary>
    /// Distribution evenness for categorical columns (0-1, where 1 is perfectly even)
    /// </summary>
    public double? DistributionEvenness { get; set; }

    /// <summary>
    /// Recommended visualizations for this column
    /// </summary>
    public List<string> RecommendedVisualizations { get; set; } = [];

    /// <summary>
    /// Data quality issues detected
    /// </summary>
    public List<string> QualityIssues { get; set; } = [];

    /// <summary>
    /// Statistical insights and recommendations
    /// </summary>
    public List<string> Insights { get; set; } = [];
}

