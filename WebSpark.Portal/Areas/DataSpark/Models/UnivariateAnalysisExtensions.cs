using Microsoft.Data.Analysis;
using System.Collections.Concurrent;

namespace WebSpark.Portal.Areas.DataSpark.Models;

public static class UnivariateAnalysisExtensions
{
    /// <summary>
    /// Analyzes individual columns of a DataFrame and generates a list of ColumnInfo objects with analysis results.
    /// </summary>
    /// <param name="dataFrame">The DataFrame containing the columns to analyze.</param>
    /// <returns>A list of ColumnInfo results for each column in the DataFrame.</returns>
    public static List<ColumnInfo> GetUnivariateAnalysis(this DataFrame dataFrame)
    {
        var columnInformationList = new ConcurrentBag<ColumnInfo>();

        // Use parallel processing for analyzing columns
        Parallel.ForEach(dataFrame.Columns, column =>
        {
            var values = column.Cast<object>().ToArray();
            var nonNullValues = values.Where(value => value != null).ToArray();
            var uniqueValues = nonNullValues.Distinct().ToArray();
            var mostCommonValue = nonNullValues
                .GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;

            // Extract numeric values efficiently
            var numericValues = nonNullValues
                .OfType<IConvertible>()
                .Where(v => AnalysisUtilities.IsNumericType(v.GetType()))
                .Select(Convert.ToDouble)
                .ToArray();

            // Calculate mean, standard deviation, and skewness for numeric columns
            var mean = numericValues.Length > 0 ? numericValues.Average() : double.NaN;
            var standardDeviation = numericValues.Length > 0 ? AnalysisUtilities.CalculateStandardDeviation(numericValues) : double.NaN;
            var skewness = numericValues.Length > 0 ? AnalysisUtilities.CalculateSkewness(numericValues) : double.NaN;

            var columnInfo = new ColumnInfo
            {
                Column = column.Name,
                Type = column.DataType.ToString(),
                NonNullCount = nonNullValues.Length,
                NullCount = values.Length - nonNullValues.Length,
                UniqueCount = uniqueValues.Length,
                MostCommonValue = mostCommonValue,
                Skewness = skewness,
                Min = numericValues.Length > 0 ? numericValues.Min() : null,
                Max = numericValues.Length > 0 ? numericValues.Max() : null,
                Mean = mean,
                StandardDeviation = standardDeviation
            };

            AnalyzeColumn(columnInfo, nonNullValues, numericValues);

            // Add the column info to the concurrent collection
            columnInformationList.Add(columnInfo);
        });

        return columnInformationList.ToList();
    }

    /// <summary>
    /// Analyzes a single DataFrame column and updates the ColumnInfo object with results.
    /// </summary>
    /// <param name="column">The ColumnInfo object representing the column to analyze.</param>
    /// <param name="nonNullValues">The array of non-null values in the column.</param>
    /// <param name="numericValues">The array of numeric values in the column.</param>
    private static void AnalyzeColumn(ColumnInfo column, object[] nonNullValues, double[] numericValues)
    {
        bool isNumeric = numericValues.Length > 0;
        bool isCategorical = column.Type == typeof(string).FullName;

        // Handle numeric columns
        if (isNumeric)
        {
            column.Observations.Add($"The column '{column.Column}' is a numeric type, suitable for quantitative analysis.");

            // Handle skewness
            if (Math.Abs(column.Skewness) > 1)
            {
                column.Observations.Add($"The column exhibits a high skewness of {column.Skewness:F2}. Consider transformations to normalize the data.");
            }
            else if (Math.Abs(column.Skewness) > 0.5)
            {
                column.Observations.Add($"Moderate skewness detected ({column.Skewness:F2}). May slightly affect tests assuming normality.");
            }
            else
            {
                column.Observations.Add($"Skewness of {column.Skewness:F2} indicates symmetric distribution, favorable for analysis.");
            }

            // Recommendations for visualizations
            column.Observations.Add("Recommended visualizations: histograms, box plots, and scatter plots.");

            // Check for outliers
            if (column.StandardDeviation > 0 && column.Min != null && column.Max != null)
            {
                double upperThreshold = column.Mean + 3 * column.StandardDeviation;
                double lowerThreshold = column.Mean - 3 * column.StandardDeviation;

                if (Convert.ToDouble(column.Max) > upperThreshold)
                {
                    column.Observations.Add("Potential high outliers detected above three standard deviations.");
                }

                if (Convert.ToDouble(column.Min) < lowerThreshold)
                {
                    column.Observations.Add("Potential low outliers detected below three standard deviations.");
                }

                if (Convert.ToDouble(column.Max) <= upperThreshold && Convert.ToDouble(column.Min) >= lowerThreshold)
                {
                    column.Observations.Add("No significant outliers detected.");
                }
            }
            else
            {
                column.Observations.Add("Insufficient data for outlier detection.");
            }

            // Additional Observations for Numeric Data
            column.Observations.Add($"Mean value is {column.Mean:F2}. This represents the central tendency of the data.");
            column.Observations.Add($"Standard deviation is {column.StandardDeviation:F2}, indicating the spread or dispersion of the data values.");
        }
        // Handle categorical columns
        else if (isCategorical)
        {
            column.Observations.Add($"The column '{column.Column}' is a categorical type, ideal for grouping and segmentation.");

            // Recommendations for visualizations
            column.Observations.Add("Recommended visualizations: bar charts, pie charts, and count plots.");

            // Check for uniqueness
            if (column.UniqueCount < column.NonNullCount)
            {
                column.Observations.Add($"Repeated values detected. {column.NonNullCount - column.UniqueCount} duplicates found.");
            }
            else
            {
                column.Observations.Add("All values are unique, suggesting potential identifiers.");
            }

            // Additional Observations for Categorical Data
            column.Observations.Add($"Most common value is '{column.MostCommonValue}', which appears frequently in the dataset.");
            column.Observations.Add($"The column contains {column.UniqueCount} unique categories.");

            // Add unique value counts if the unique count is less than 20
            if (column.UniqueCount < 20)
            {
                var valueCounts = nonNullValues
                    .GroupBy(v => v)
                    .OrderByDescending(g => g.Count())
                    .Select(g => $"{g.Key}: {g.Count()}")
                    .ToList();

                column.Observations.Add("Values (in descending order of counts): " + string.Join(", ", valueCounts));
            }
        }
        else
        {
            column.Observations.Add($"Column '{column.Column}' of type '{column.Type}' is not commonly analyzed directly. Consider feature extraction.");
        }

        // Check for missing values
        if (column.NullCount > 0)
        {
            column.Observations.Add($"The column contains {column.NullCount} missing values. Consider handling these before analysis.");
        }
        else
        {
            column.Observations.Add("No missing values detected.");
        }

        // Check for duplicates
        if (column.UniqueCount < column.NonNullCount)
        {
            column.Observations.Add($"Detected {column.NonNullCount - column.UniqueCount} duplicate values, which could affect the analysis.");
        }

        // Additional General Observations
        if (column.Type == typeof(DateTime).FullName)
        {
            column.Observations.Add($"The column '{column.Column}' is a DateTime type. Time series analysis and trends over time could be valuable.");
        }

        if (column.NonNullCount == 0)
        {
            column.Observations.Add("The column contains no non-null values, making it unsuitable for analysis without further data cleaning.");
        }
    }
}
