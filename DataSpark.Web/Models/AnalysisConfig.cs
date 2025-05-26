namespace DataSpark.Web.Models;


/// <summary>
/// Configuration class for customizing univariate analysis behavior.
/// </summary>
public class AnalysisConfig
{
    // Existing thresholds
    public long NonUniqueThresholdForHistograms { get; set; } = 50;
    public long NonUniqueThresholdForBoxPlots { get; set; } = 20;
    public double HighSkewnessThreshold { get; set; } = 1.0;
    public double ModerateSkewnessThreshold { get; set; } = 0.5;
    public int UniqueCountThreshold { get; set; } = 20;
    public double OutlierStdDevMultiplier { get; set; } = 3.0;

    // New configuration options for enhanced analysis

    /// <summary>
    /// Threshold for determining if a column has too many categories to show all frequencies
    /// </summary>
    public int MaxCategoriesForFullDisplay { get; set; } = 15;

    /// <summary>
    /// Minimum percentage of values that must be numeric to treat column as numeric
    /// </summary>
    public double MinNumericPercentage { get; set; } = 80.0;

    /// <summary>
    /// Threshold ratio of unique values to total values for considering column as high-cardinality
    /// </summary>
    public double HighCardinalityThreshold { get; set; } = 0.9;

    /// <summary>
    /// Threshold ratio of unique values to total values for considering column as low-information
    /// </summary>
    public double LowInformationThreshold { get; set; } = 0.01;

    /// <summary>
    /// Coefficient of variation threshold for classifying variability as high
    /// </summary>
    public double HighVariabilityThreshold { get; set; } = 100.0;

    /// <summary>
    /// Coefficient of variation threshold for classifying variability as moderate
    /// </summary>
    public double ModerateVariabilityThreshold { get; set; } = 30.0;

    /// <summary>
    /// Percentage threshold for classifying missing values as high
    /// </summary>
    public double HighMissingValueThreshold { get; set; } = 50.0;

    /// <summary>
    /// Percentage threshold for classifying missing values as moderate
    /// </summary>
    public double ModerateMissingValueThreshold { get; set; } = 20.0;

    /// <summary>
    /// Percentage threshold for classifying duplicate values as high
    /// </summary>
    public double HighDuplicateThreshold { get; set; } = 80.0;

    /// <summary>
    /// Percentage threshold for classifying duplicate values as moderate
    /// </summary>
    public double ModerateDuplicateThreshold { get; set; } = 50.0;

    /// <summary>
    /// Minimum data completeness percentage for considering data quality as good
    /// </summary>
    public double GoodDataQualityThreshold { get; set; } = 80.0;

    /// <summary>
    /// Minimum data completeness percentage for considering data quality as high
    /// </summary>
    public double HighDataQualityThreshold { get; set; } = 95.0;
}
