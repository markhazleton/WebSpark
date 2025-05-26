using Microsoft.Data.Analysis;
using System.Collections.Concurrent;

namespace DataSpark.Web.Models;

public static class UnivariateAnalysisExtensions
{
    /// <summary>
    /// Analyzes individual columns of a DataFrame and generates a list of ColumnInfo objects with analysis results.
    /// </summary>
    /// <param name="dataFrame">The DataFrame containing the columns to analyze.</param>
    /// <param name="config">Optional configuration for analysis behavior.</param>
    /// <returns>A list of ColumnInfo results for each column in the DataFrame.</returns>
    public static List<ColumnInfo> GetUnivariateAnalysis(this DataFrame dataFrame, AnalysisConfig? config = null)
    {
        config ??= new AnalysisConfig(); // Use default config if none provided
        var columnInformationList = new ConcurrentBag<ColumnInfo>();

        // Use parallel processing for analyzing columns
        Parallel.ForEach(dataFrame.Columns, column =>
        {
            try
            {
                columnInformationList.Add(GetColumnAnalysis(config, column));
            }
            catch (Exception ex)
            {
                // Handle errors gracefully, logging as needed
                Console.WriteLine($"Error analyzing column {column.Name}: {ex.Message}");
            }
        });

        return [.. columnInformationList];
    }
    private static ColumnInfo GetColumnAnalysis(AnalysisConfig config, DataFrameColumn column)
    {
        var values = column.Cast<object>().ToArray();
        var nonNullValues = values.Where(value => value != null).ToArray();
        var uniqueValues = nonNullValues.Distinct().ToArray();

        // Calculate mode and its frequency
        var modeGroup = nonNullValues
            .GroupBy(x => x)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        var mostCommonValue = modeGroup?.Key;
        var modeFrequency = modeGroup?.Count() ?? 0;

        // Extract numeric values with optimized type checking
        var numericValues = nonNullValues
            .Where(v => v is byte or short or int or long or float or double or decimal)
            .Select(Convert.ToDouble)
            .ToArray();

        // Calculate basic statistics
        var mean = numericValues.Length > 0 ? numericValues.Average() : double.NaN;
        var standardDeviation = numericValues.Length > 0 ? AnalysisUtilities.CalculateStandardDeviation(numericValues) : double.NaN;
        var skewness = numericValues.Length > 0 ? AnalysisUtilities.CalculateSkewness(numericValues) : double.NaN;

        // Calculate enhanced statistics for numeric columns
        double? median = null, q1 = null, q3 = null, iqr = null, coefficientOfVariation = null;
        if (numericValues.Length > 0)
        {
            var sorted = numericValues.OrderBy(x => x).ToArray();
            median = CalculatePercentile(sorted, 50);
            q1 = CalculatePercentile(sorted, 25);
            q3 = CalculatePercentile(sorted, 75);
            iqr = q3 - q1;
            coefficientOfVariation = Math.Abs(mean) > 0 ? (standardDeviation / Math.Abs(mean)) * 100 : 0;
        }

        // Calculate percentages and quality metrics
        var totalCount = values.Length;
        var missingPercentage = totalCount > 0 ? (double)values.Count(v => v == null) / totalCount * 100 : 0;
        var duplicateCount = nonNullValues.Length - uniqueValues.Length;
        var duplicatePercentage = nonNullValues.Length > 0 ? (double)duplicateCount / nonNullValues.Length * 100 : 0;

        // Calculate data quality score
        var dataQualityScore = CalculateDataQualityScore(missingPercentage, duplicatePercentage, config);

        // Calculate information content score
        var informationContentScore = CalculateInformationContentScore(uniqueValues.Length, nonNullValues.Length);

        var columnInfo = new ColumnInfo
        {
            Column = column.Name,
            Type = column.DataType.ToString(),
            NonNullCount = nonNullValues.Length,
            NullCount = values.Length - nonNullValues.Length,
            UniqueCount = uniqueValues.Length,
            MostCommonValue = mostCommonValue,
            Mode = mostCommonValue,
            ModeFrequency = modeFrequency,
            Skewness = skewness,
            Min = numericValues.Length > 0 ? numericValues.Min() : null,
            Max = numericValues.Length > 0 ? numericValues.Max() : null,
            Mean = mean,
            StandardDeviation = standardDeviation,
            Median = median,
            Q1 = q1,
            Q3 = q3,
            IQR = iqr,
            CoefficientOfVariation = coefficientOfVariation,
            MissingPercentage = missingPercentage,
            DuplicatePercentage = duplicatePercentage,
            DataQualityScore = dataQualityScore,
            InformationContentScore = informationContentScore
        };

        AnalyzeColumn(columnInfo, nonNullValues, numericValues, config);
        return columnInfo;
    }

    /// <summary>
    /// Calculates a data quality score based on completeness and duplication
    /// </summary>
    private static double CalculateDataQualityScore(double missingPercentage, double duplicatePercentage, AnalysisConfig config)
    {
        double completenessScore = Math.Max(0, 100 - missingPercentage);
        double uniquenessScore = Math.Max(0, 100 - duplicatePercentage);

        // Weight completeness more heavily than uniqueness
        return (completenessScore * 0.7) + (uniquenessScore * 0.3);
    }

    /// <summary>
    /// Calculates an information content score based on uniqueness ratio
    /// </summary>
    private static double CalculateInformationContentScore(int uniqueCount, int nonNullCount)
    {
        if (nonNullCount == 0) return 0;

        double uniqueRatio = (double)uniqueCount / nonNullCount;

        // Optimal information content is around 0.1 to 0.8 unique ratio
        // Too low (repetitive) or too high (all unique) reduces information value
        if (uniqueRatio <= 0.01) return 20; // Very low information
        if (uniqueRatio >= 0.99) return 30; // Likely identifier, limited analytical value
        if (uniqueRatio >= 0.1 && uniqueRatio <= 0.8) return 100; // Optimal range

        // Scale between optimal ranges
        if (uniqueRatio < 0.1) return 20 + (uniqueRatio / 0.1) * 80;
        return 100 - ((uniqueRatio - 0.8) / 0.19) * 70;
    }

    /// <summary>
    /// Analyzes a single DataFrame column and updates the ColumnInfo object with results.
    /// </summary>
    /// <param name="column">The ColumnInfo object representing the column to analyze.</param>
    /// <param name="nonNullValues">The array of non-null values in the column.</param>
    /// <param name="numericValues">The array of numeric values in the column.</param>
    /// <param name="config">Configuration object with analysis settings.</param>
    private static void AnalyzeColumn(
        ColumnInfo column,
        object[] nonNullValues,
        double[] numericValues,
        AnalysisConfig config)
    {
        bool isNumeric = numericValues.Length > 0 && numericValues.Length == nonNullValues.Length;
        bool isCategorical = column.Type == typeof(string).FullName || !isNumeric;

        // Enhanced missing values analysis
        AnalyzeMissingValues(column);

        // Enhanced duplicate analysis
        AnalyzeDuplicates(column, nonNullValues);

        // Handle numeric columns with comprehensive analysis
        if (isNumeric)
        {
            AnalyzeNumericColumn(column, numericValues, config);
        }
        // Handle categorical columns
        else
        {
            AnalyzeCategoricalColumn(column, nonNullValues, config);
        }

        // General observations
        AddGeneralObservations(column);
    }

    /// <summary>
    /// Performs comprehensive analysis for numeric columns
    /// </summary>
    private static void AnalyzeNumericColumn(ColumnInfo column, double[] numericValues, AnalysisConfig config)
    {
        column.Observations.Add($"The column '{column.Column}' is numeric with {numericValues.Length} valid values, suitable for quantitative analysis.");

        // Distribution analysis
        AnalyzeDistribution(column, numericValues, config);

        // Central tendency analysis
        AnalyzeCentralTendency(column, numericValues);

        // Variability analysis  
        AnalyzeVariability(column, numericValues, config);

        // Outlier analysis using multiple methods
        AnalyzeOutliers(column, numericValues, config);

        // Quartile and percentile analysis
        AnalyzeQuartiles(column, numericValues);

        // Visualization recommendations
        RecommendNumericVisualizations(column, config);
    }

    /// <summary>
    /// Performs comprehensive analysis for categorical columns
    /// </summary>
    private static void AnalyzeCategoricalColumn(ColumnInfo column, object[] nonNullValues, AnalysisConfig config)
    {
        column.Observations.Add($"The column '{column.Column}' is categorical with {column.UniqueCount} distinct categories, ideal for grouping and segmentation.");

        // Mode and frequency analysis
        AnalyzeCategoricalFrequencies(column, nonNullValues, config);

        // Category distribution analysis
        AnalyzeCategoryDistribution(column, nonNullValues);

        // Visualization recommendations
        RecommendCategoricalVisualizations(column, config);
    }

    /// <summary>
    /// Analyzes missing values with detailed insights
    /// </summary>
    private static void AnalyzeMissingValues(ColumnInfo column)
    {
        if (column.NullCount > 0)
        {
            double missingPercentage = (double)column.NullCount / (column.NonNullCount + column.NullCount) * 100;

            if (missingPercentage > 50)
            {
                column.Observations.Add($"⚠️ HIGH MISSING VALUES: {column.NullCount} ({missingPercentage:F1}%) missing values detected. Column may be unreliable for analysis.");
            }
            else if (missingPercentage > 20)
            {
                column.Observations.Add($"⚠️ MODERATE MISSING VALUES: {column.NullCount} ({missingPercentage:F1}%) missing values. Consider imputation strategies.");
            }
            else
            {
                column.Observations.Add($"✓ LOW MISSING VALUES: {column.NullCount} ({missingPercentage:F1}%) missing values detected. Generally acceptable for analysis.");
            }
        }
        else
        {
            column.Observations.Add("✓ COMPLETE DATA: No missing values detected - excellent data quality.");
        }
    }

    /// <summary>
    /// Analyzes duplicate values with insights
    /// </summary>
    private static void AnalyzeDuplicates(ColumnInfo column, object[] nonNullValues)
    {
        if (column.UniqueCount < column.NonNullCount)
        {
            long duplicateCount = column.NonNullCount - column.UniqueCount;
            double duplicatePercentage = (double)duplicateCount / column.NonNullCount * 100;

            if (duplicatePercentage > 80)
            {
                column.Observations.Add($"⚠️ HIGH REPETITION: {duplicateCount} duplicate values ({duplicatePercentage:F1}%). Data may have low information content.");
            }
            else if (duplicatePercentage > 50)
            {
                column.Observations.Add($"⚠️ MODERATE REPETITION: {duplicateCount} duplicate values ({duplicatePercentage:F1}%). Consider if this affects analysis.");
            }
            else
            {
                column.Observations.Add($"✓ ACCEPTABLE REPETITION: {duplicateCount} duplicate values ({duplicatePercentage:F1}%) found.");
            }
        }
        else
        {
            column.Observations.Add("✓ ALL UNIQUE VALUES: Each value is unique - possible identifier column or high-resolution data.");
        }
    }

    /// <summary>
    /// Analyzes distribution characteristics for numeric data
    /// </summary>
    private static void AnalyzeDistribution(ColumnInfo column, double[] numericValues, AnalysisConfig config)
    {
        // Skewness analysis with enhanced interpretation
        if (Math.Abs(column.Skewness) > config.HighSkewnessThreshold)
        {
            string direction = column.Skewness > 0 ? "right (positive)" : "left (negative)";
            column.Observations.Add($"📊 HIGHLY SKEWED: Distribution is skewed {direction} (skewness: {column.Skewness:F2}). Consider log transformation or robust statistics.");
        }
        else if (Math.Abs(column.Skewness) > config.ModerateSkewnessThreshold)
        {
            string direction = column.Skewness > 0 ? "right" : "left";
            column.Observations.Add($"📊 MODERATELY SKEWED: Slight {direction} skew (skewness: {column.Skewness:F2}). May affect normality assumptions.");
        }
        else
        {
            column.Observations.Add($"📊 SYMMETRIC DISTRIBUTION: Well-balanced distribution (skewness: {column.Skewness:F2}) - favorable for most statistical analyses.");
        }

        // Range analysis
        double range = Convert.ToDouble(column.Max) - Convert.ToDouble(column.Min);
        column.Observations.Add($"📏 RANGE: Data spans from {column.Min} to {column.Max} (range: {range:F2}).");
    }

    /// <summary>
    /// Analyzes central tendency measures
    /// </summary>
    private static void AnalyzeCentralTendency(ColumnInfo column, double[] numericValues)
    {
        // Calculate mode for numeric data
        var mode = CalculateMode(numericValues);

        column.Observations.Add($"📈 CENTRAL TENDENCY: Mean = {column.Mean:F2}, representing the average value.");

        if (mode.HasValue)
        {
            // Compare mean and mode
            double meanModeDistance = Math.Abs(column.Mean - mode.Value);
            double relativeDistance = meanModeDistance / column.StandardDeviation;

            if (relativeDistance < 0.5)
            {
                column.Observations.Add($"📈 MODE: Most frequent value is {mode.Value:F2}, close to the mean - indicates symmetric distribution.");
            }
            else
            {
                column.Observations.Add($"📈 MODE: Most frequent value is {mode.Value:F2}, differs significantly from mean - indicates skewed distribution.");
            }
        }
        else
        {
            column.Observations.Add("📈 MODE: No dominant mode detected - values are well distributed.");
        }
    }

    /// <summary>
    /// Analyzes variability and spread
    /// </summary>
    private static void AnalyzeVariability(ColumnInfo column, double[] numericValues, AnalysisConfig config)
    {
        // Coefficient of variation
        double coefficientOfVariation = Math.Abs(column.Mean) > 0 ? (column.StandardDeviation / Math.Abs(column.Mean)) * 100 : 0;

        if (coefficientOfVariation > 100)
        {
            column.Observations.Add($"📊 HIGH VARIABILITY: Coefficient of variation is {coefficientOfVariation:F1}% - data is highly dispersed relative to the mean.");
        }
        else if (coefficientOfVariation > 30)
        {
            column.Observations.Add($"📊 MODERATE VARIABILITY: Coefficient of variation is {coefficientOfVariation:F1}% - moderate dispersion around the mean.");
        }
        else
        {
            column.Observations.Add($"📊 LOW VARIABILITY: Coefficient of variation is {coefficientOfVariation:F1}% - data is tightly clustered around the mean.");
        }

        column.Observations.Add($"📊 SPREAD: Standard deviation is {column.StandardDeviation:F2}, indicating the typical deviation from the mean.");
    }

    /// <summary>
    /// Enhanced outlier detection using multiple methods
    /// </summary>
    private static void AnalyzeOutliers(ColumnInfo column, double[] numericValues, AnalysisConfig config)
    {
        // Method 1: Standard deviation method
        AnalyzeOutliersStdDev(column, numericValues, config);

        // Method 2: IQR method
        AnalyzeOutliersIQR(column, numericValues);
    }

    /// <summary>
    /// Standard deviation outlier detection
    /// </summary>
    private static void AnalyzeOutliersStdDev(ColumnInfo column, double[] numericValues, AnalysisConfig config)
    {
        if (column.StandardDeviation > 0 && column.Min != null && column.Max != null)
        {
            double upperThreshold = column.Mean + config.OutlierStdDevMultiplier * column.StandardDeviation;
            double lowerThreshold = column.Mean - config.OutlierStdDevMultiplier * column.StandardDeviation;

            int highOutliers = numericValues.Count(v => v > upperThreshold);
            int lowOutliers = numericValues.Count(v => v < lowerThreshold);
            int totalOutliers = highOutliers + lowOutliers;

            if (totalOutliers > 0)
            {
                double outlierPercentage = (double)totalOutliers / numericValues.Length * 100;
                column.Observations.Add($"⚠️ OUTLIERS (±{config.OutlierStdDevMultiplier}σ): {totalOutliers} outliers detected ({outlierPercentage:F1}%) - {highOutliers} high, {lowOutliers} low.");
            }
            else
            {
                column.Observations.Add($"✓ NO EXTREME OUTLIERS: No values beyond ±{config.OutlierStdDevMultiplier} standard deviations detected.");
            }
        }
    }

    /// <summary>
    /// IQR outlier detection method
    /// </summary>
    private static void AnalyzeOutliersIQR(ColumnInfo column, double[] numericValues)
    {
        var sorted = numericValues.OrderBy(x => x).ToArray();
        double q1 = CalculatePercentile(sorted, 25);
        double q3 = CalculatePercentile(sorted, 75);
        double iqr = q3 - q1;

        double lowerFence = q1 - 1.5 * iqr;
        double upperFence = q3 + 1.5 * iqr;

        int lowIQROutliers = numericValues.Count(v => v < lowerFence);
        int highIQROutliers = numericValues.Count(v => v > upperFence);
        int totalIQROutliers = lowIQROutliers + highIQROutliers;

        if (totalIQROutliers > 0)
        {
            double outlierPercentage = (double)totalIQROutliers / numericValues.Length * 100;
            column.Observations.Add($"⚠️ OUTLIERS (IQR): {totalIQROutliers} outliers detected ({outlierPercentage:F1}%) using interquartile range method.");
        }
        else
        {
            column.Observations.Add("✓ NO IQR OUTLIERS: No outliers detected using interquartile range method.");
        }
    }

    /// <summary>
    /// Analyzes quartiles and percentiles
    /// </summary>
    private static void AnalyzeQuartiles(ColumnInfo column, double[] numericValues)
    {
        var sorted = numericValues.OrderBy(x => x).ToArray();

        double q1 = CalculatePercentile(sorted, 25);
        double median = CalculatePercentile(sorted, 50);
        double q3 = CalculatePercentile(sorted, 75);
        double p90 = CalculatePercentile(sorted, 90);
        double p95 = CalculatePercentile(sorted, 95);

        column.Observations.Add($"📊 QUARTILES: Q1={q1:F2}, Median={median:F2}, Q3={q3:F2}");
        column.Observations.Add($"📊 PERCENTILES: 90th={p90:F2}, 95th={p95:F2}");

        // Analyze quartile relationships
        double iqr = q3 - q1;
        if (Math.Abs(median - column.Mean) / column.StandardDeviation < 0.1)
        {
            column.Observations.Add("📊 QUARTILE ANALYSIS: Median closely matches mean - suggests symmetric distribution.");
        }
        else if (median < column.Mean)
        {
            column.Observations.Add("📊 QUARTILE ANALYSIS: Median below mean - suggests right-skewed distribution.");
        }
        else
        {
            column.Observations.Add("📊 QUARTILE ANALYSIS: Median above mean - suggests left-skewed distribution.");
        }
    }

    /// <summary>
    /// Analyzes categorical frequencies and mode
    /// </summary>
    private static void AnalyzeCategoricalFrequencies(ColumnInfo column, object[] nonNullValues, AnalysisConfig config)
    {
        var frequencies = nonNullValues
            .GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .ToList();

        // Mode analysis
        var topCategory = frequencies.First();
        double modeFrequency = (double)topCategory.Count() / nonNullValues.Length * 100;

        column.Observations.Add($"📈 MODE: Most frequent category is '{topCategory.Key}' ({topCategory.Count()} occurrences, {modeFrequency:F1}%).");

        // Frequency distribution analysis
        if (frequencies.Count <= 5)
        {
            column.Observations.Add("📊 DISTRIBUTION: Small number of categories - highly concentrated data.");

            // Show all frequencies for small datasets
            var allFreqs = frequencies.Select(f => $"'{f.Key}': {f.Count()}").ToList();
            column.Observations.Add($"📊 FREQUENCIES: {string.Join(", ", allFreqs)}");
        }
        else if (frequencies.Count <= config.UniqueCountThreshold)
        {
            column.Observations.Add("📊 DISTRIBUTION: Moderate number of categories - good for analysis.");

            // Show top frequencies
            var topFreqs = frequencies.Take(5).Select(f => $"'{f.Key}': {f.Count()}").ToList();
            column.Observations.Add($"📊 TOP FREQUENCIES: {string.Join(", ", topFreqs)}");
        }
        else
        {
            column.Observations.Add($"📊 DISTRIBUTION: Large number of categories ({frequencies.Count}) - may need grouping for analysis.");

            // Show top 3 frequencies
            var topFreqs = frequencies.Take(3).Select(f => $"'{f.Key}': {f.Count()}").ToList();
            column.Observations.Add($"📊 TOP 3 FREQUENCIES: {string.Join(", ", topFreqs)}");
        }
    }

    /// <summary>
    /// Analyzes category distribution patterns
    /// </summary>
    private static void AnalyzeCategoryDistribution(ColumnInfo column, object[] nonNullValues)
    {
        var frequencies = nonNullValues
            .GroupBy(v => v)
            .Select(g => g.Count())
            .OrderByDescending(c => c)
            .ToArray();

        // Calculate distribution evenness
        double entropy = CalculateEntropy(frequencies);
        double maxEntropy = Math.Log2(frequencies.Length);
        double evenness = maxEntropy > 0 ? entropy / maxEntropy : 0;

        if (evenness > 0.8)
        {
            column.Observations.Add("📊 EVEN DISTRIBUTION: Categories are fairly evenly distributed - good for balanced analysis.");
        }
        else if (evenness > 0.5)
        {
            column.Observations.Add("📊 MODERATE DISTRIBUTION: Some categories dominate but distribution is reasonably balanced.");
        }
        else
        {
            column.Observations.Add("📊 UNEVEN DISTRIBUTION: Distribution is heavily skewed toward few categories - consider grouping rare categories.");
        }
    }

    /// <summary>
    /// Provides visualization recommendations for numeric columns
    /// </summary>
    private static void RecommendNumericVisualizations(ColumnInfo column, AnalysisConfig config)
    {
        var recommendations = new List<string>();

        if (column.UniqueCount == column.NonNullCount)
        {
            recommendations.AddRange(["scatter plots", "line plots", "density plots"]);
            column.Observations.Add("📈 VIZ RECOMMENDATION: All values unique - use scatter/line plots to show individual data points.");
        }
        else if (column.UniqueCount > config.NonUniqueThresholdForHistograms)
        {
            recommendations.AddRange(["histograms", "density plots", "box plots"]);
            column.Observations.Add("📈 VIZ RECOMMENDATION: Many unique values - histograms and density plots will show distribution shape effectively.");
        }
        else if (column.UniqueCount > config.NonUniqueThresholdForBoxPlots)
        {
            recommendations.AddRange(["box plots", "violin plots", "histograms"]);
            column.Observations.Add("📈 VIZ RECOMMENDATION: Moderate uniqueness - box plots and violin plots will highlight quartiles and outliers.");
        }
        else
        {
            recommendations.AddRange(["bar charts", "dot plots", "box plots"]);
            column.Observations.Add("📈 VIZ RECOMMENDATION: Few unique values - bar charts will clearly show frequency of each value.");
        }
    }

    /// <summary>
    /// Provides visualization recommendations for categorical columns
    /// </summary>
    private static void RecommendCategoricalVisualizations(ColumnInfo column, AnalysisConfig config)
    {
        if (column.UniqueCount <= 10)
        {
            column.Observations.Add("📈 VIZ RECOMMENDATION: Few categories - use pie charts, bar charts, or donut charts for clear comparison.");
        }
        else if (column.UniqueCount <= 30)
        {
            column.Observations.Add("📈 VIZ RECOMMENDATION: Moderate categories - use horizontal bar charts or treemaps for better readability.");
        }
        else
        {
            column.Observations.Add("📈 VIZ RECOMMENDATION: Many categories - consider grouping rare categories or use word clouds for text data.");
        }
    }

    /// <summary>
    /// Adds general observations about the column
    /// </summary>
    private static void AddGeneralObservations(ColumnInfo column)
    {
        // Data completeness assessment
        double completeness = (double)column.NonNullCount / (column.NonNullCount + column.NullCount) * 100;

        if (completeness == 100)
        {
            column.Observations.Add("✅ EXCELLENT DATA QUALITY: Complete data with no missing values.");
        }
        else if (completeness >= 95)
        {
            column.Observations.Add("✅ HIGH DATA QUALITY: Very few missing values - excellent for analysis.");
        }
        else if (completeness >= 80)
        {
            column.Observations.Add("⚠️ GOOD DATA QUALITY: Some missing values but generally usable for analysis.");
        }
        else
        {
            column.Observations.Add("⚠️ POOR DATA QUALITY: Significant missing values - handle with caution.");
        }

        // Information content assessment
        double uniqueRatio = (double)column.UniqueCount / Math.Max(column.NonNullCount, 1);

        if (uniqueRatio == 1.0)
        {
            column.Observations.Add("🔍 IDENTIFIER COLUMN: All values unique - likely an ID or key column.");
        }
        else if (uniqueRatio > 0.9)
        {
            column.Observations.Add("🔍 HIGH RESOLUTION: Very diverse values - high information content.");
        }
        else if (uniqueRatio < 0.01)
        {
            column.Observations.Add("🔍 LOW INFORMATION: Very few unique values - limited analytical value.");
        }

        // DateTime specific observations
        if (column.Type?.Contains("DateTime") == true)
        {
            column.Observations.Add("📅 TEMPORAL DATA: DateTime column detected - suitable for time series analysis and trend identification.");
        }
    }

    /// <summary>
    /// Calculates mode for numeric data
    /// </summary>
    private static double? CalculateMode(double[] values)
    {
        var frequencies = values.GroupBy(x => x).ToList();
        var maxFrequency = frequencies.Max(g => g.Count());

        // Only return mode if it appears more than once
        if (maxFrequency == 1) return null;

        var modes = frequencies.Where(g => g.Count() == maxFrequency).Select(g => g.Key).ToList();

        // Return the first mode if multiple exist
        return modes.FirstOrDefault();
    }

    /// <summary>
    /// Calculates a specific percentile from sorted data
    /// </summary>
    private static double CalculatePercentile(double[] sortedValues, double percentile)
    {
        if (sortedValues.Length == 0) return 0;
        if (sortedValues.Length == 1) return sortedValues[0];

        double position = (percentile / 100.0) * (sortedValues.Length - 1);
        int lowerIndex = (int)Math.Floor(position);
        int upperIndex = (int)Math.Ceiling(position);

        if (lowerIndex == upperIndex)
        {
            return sortedValues[lowerIndex];
        }

        double weight = position - lowerIndex;
        return sortedValues[lowerIndex] * (1 - weight) + sortedValues[upperIndex] * weight;
    }

    /// <summary>
    /// Calculates Shannon entropy for distribution evenness
    /// </summary>
    private static double CalculateEntropy(int[] frequencies)
    {
        int total = frequencies.Sum();
        if (total == 0) return 0;

        return frequencies
            .Where(f => f > 0)
            .Select(f => (double)f / total)
            .Select(p => -p * Math.Log2(p))
            .Sum();
    }
}
