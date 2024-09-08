using Microsoft.Data.Analysis;

namespace WebSpark.Portal.Areas.DataSpark.Models;

public static class AnalysisUtilities
{
    /// <summary>
    /// Checks if the given type is numeric.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is numeric; otherwise, false.</returns>
    public static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
               type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte) ||
               type == typeof(float) || type == typeof(double) || type == typeof(decimal);
    }

    /// <summary>
    /// Calculates the standard deviation of an array of values.
    /// </summary>
    /// <param name="values">The array of numeric values.</param>
    /// <returns>The standard deviation of the values.</returns>
    public static double CalculateStandardDeviation(double[] values)
    {
        if (values.Length == 0) return double.NaN;

        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Length;
        return Math.Sqrt(variance);
    }

    /// <summary>
    /// Calculates the skewness of an array of values.
    /// </summary>
    /// <param name="values">The array of numeric values.</param>
    /// <returns>The skewness of the values.</returns>
    public static double CalculateSkewness(double[] values)
    {
        if (values.Length == 0) return double.NaN;

        var mean = values.Average();
        var stdDev = CalculateStandardDeviation(values);
        if (stdDev == 0) return 0;

        var skewness = values.Sum(v => Math.Pow((v - mean) / stdDev, 3)) / values.Length;
        return skewness;
    }

    /// <summary>
    /// Calculates the correlation between two numeric arrays.
    /// </summary>
    /// <param name="values1">The first array of numeric values.</param>
    /// <param name="values2">The second array of numeric values.</param>
    /// <returns>The correlation coefficient between the two arrays.</returns>
    public static double CalculateCorrelation(double[] values1, double[] values2)
    {
        var mean1 = values1.Average();
        var mean2 = values2.Average();

        var sumProduct = values1.Zip(values2, (v1, v2) => (v1 - mean1) * (v2 - mean2)).Sum();
        var sumSquare1 = values1.Sum(v => Math.Pow(v - mean1, 2));
        var sumSquare2 = values2.Sum(v => Math.Pow(v - mean2, 2));

        var correlation = sumProduct / Math.Sqrt(sumSquare1 * sumSquare2);
        return correlation;
    }

    /// <summary>
    /// Calculates the noise factor based on the standard deviation and mean of the values.
    /// </summary>
    /// <param name="values">The numeric values to analyze for noise.</param>
    /// <returns>A noise factor between 0 and 1, where 1 indicates maximum noise.</returns>
    public static double CalculateNoiseFactor(double[] values)
    {
        if (values.Length == 0)
            return 1.0; // Maximum noise for empty or highly irregular data

        double mean = values.Average();
        double stdDev = CalculateStandardDeviation(values);

        // Use a noise ratio: standard deviation as a proportion of the mean
        double noiseRatio = mean != 0 ? stdDev / Math.Abs(mean) : 1.0;

        // Define thresholds for noise (adjust based on your specific context)
        double noiseThreshold = 0.5; // Example threshold
        double excessiveNoiseThreshold = 1.0; // Excessive noise threshold

        // Calculate noise factor where higher means more noise
        if (noiseRatio > excessiveNoiseThreshold)
            return 1.0; // Maximum noise
        if (noiseRatio > noiseThreshold)
            return 0.75 + 0.25 * (noiseRatio - noiseThreshold) / (excessiveNoiseThreshold - noiseThreshold);

        // Low noise
        return Math.Clamp(1.0 - noiseRatio, 0.0, 1.0);
    }

    /// <summary>
    /// Calculates the outlier factor based on the number of outliers beyond three standard deviations.
    /// </summary>
    /// <param name="values">The numeric values to analyze for outliers.</param>
    /// <returns>An outlier factor between 0 and 1, where 1 indicates no outliers.</returns>
    public static double CalculateOutlierFactor(double[] values)
    {
        if (values.Length == 0)
            return 1.0;

        double mean = values.Average();
        double stdDev = CalculateStandardDeviation(values);
        double lowerBound = mean - 3 * stdDev;
        double upperBound = mean + 3 * stdDev;

        int outlierCount = values.Count(v => v < lowerBound || v > upperBound);
        double outlierRatio = (double)outlierCount / values.Length;

        // Higher factor for more outliers
        return Math.Clamp(1.0 - outlierRatio, 0.0, 1.0);
    }

    /// <summary>
    /// Calculates the insight score based on correlation, uniqueness, p-value, and noise factors.
    /// </summary>
    /// <param name="correlation">The correlation between two variables.</param>
    /// <param name="uniqueCount1">The number of unique values in the first variable.</param>
    /// <param name="uniqueCount2">The number of unique values in the second variable.</param>
    /// <param name="totalCount">The total number of observations.</param>
    /// <param name="pValue">The p-value from a statistical test.</param>
    /// <param name="noiseFactor">The combined noise factor from data variability and outliers.</param>
    /// <returns>A percentage score representing the strength and reliability of the insights.</returns>
    public static double CalculateInsightScore(double correlation, int uniqueCount1, int uniqueCount2, long totalCount, double pValue = 1.0, double noiseFactor = 0.0)
    {
        // Scale correlation to be between 0 and 1 (absolute value)
        double scaledCorrelation = Math.Abs(correlation);

        // Adjust score based on the p-value (e.g., significance level)
        double pValueAdjustment = Math.Max(0.1, 1 - pValue); // Ensure it's within a reasonable range to avoid zero

        // Consider the uniqueness of values (normalized by the total count of rows)
        double uniquenessScore = 1.0 - Math.Min((double)(uniqueCount1 * uniqueCount2) / (totalCount * totalCount), 1.0);

        // Apply noise penalty
        double noisePenalty = Math.Clamp(1.0 - noiseFactor, 0.0, 1.0); // Less noise means higher score

        // Combine factors using weights for a more granular and comprehensive score
        double combinedScore = 0.5 * scaledCorrelation + 0.3 * pValueAdjustment + 0.2 * uniquenessScore;

        // Final score adjusted by noise
        double insightScore = combinedScore * noisePenalty;

        // Ensure score remains within 0 to 1 range and adjust to a percentage
        return Math.Clamp(insightScore, 0.0, 1.0) * 100;
    }

    /// <summary>
    /// Determines if a DataFrame column is categorical based on its type, unique values, and distribution.
    /// </summary>
    /// <param name="column">The DataFrame column to evaluate.</param>
    /// <param name="threshold">Optional threshold to classify numeric columns as categorical based on the number of unique values.</param>
    /// <returns>True if the column is considered categorical; otherwise, false.</returns>
    public static bool IsCategorical(DataFrameColumn column, int threshold = 20)
    {
        // Check if the column is of a type typically used for categorical data
        var columnType = column.DataType;
        if (columnType == typeof(string) || columnType == typeof(bool))
        {
            return true;
        }

        // Check for numeric types that might represent categories (e.g., integers with limited range)
        if (IsNumericType(columnType))
        {
            // Consider numeric columns as categorical if they have limited unique values
            var uniqueValueCount = column.Cast<object>().Distinct().Count();
            var totalValues = column.Length;

            // Check if the proportion of unique values is low compared to total values
            double uniqueRatio = (double)uniqueValueCount / totalValues;
            if (uniqueValueCount <= threshold || uniqueRatio < 0.1)
            {
                return true; // Likely categorical due to limited unique values or low unique ratio
            }
        }

        // Check for enums or other types that could represent categories
        if (columnType.IsEnum)
        {
            return true;
        }

        // Additional heuristic: check if the column has a small set of repeating distinct values
        if (columnType == typeof(object))
        {
            // If the column is an object type, check for a limited set of distinct values
            var uniqueValues = column.Cast<object>().Where(value => value != null).Distinct().ToArray();
            if (uniqueValues.Length <= threshold)
            {
                return true; // Consider categorical due to limited distinct values
            }
        }

        // Default: not considered categorical
        return false;
    }
}
