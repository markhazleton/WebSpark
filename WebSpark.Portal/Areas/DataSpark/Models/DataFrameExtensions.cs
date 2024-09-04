using Microsoft.Data.Analysis;

namespace WebSpark.Portal.Areas.DataSpark.Models;

public static class DataFrameExtensions
{
    public static List<ColumnInfo> GetColumnAnalysis(this DataFrame dataFrame)
    {
        // Generate basic column information and statistics
        var columnInfos = dataFrame.Columns.Select(column =>
        {
            var values = column.Cast<object>().ToArray();
            var nonNullValues = values.Where(value => value != null).ToList();
            var uniqueValues = nonNullValues.Distinct().ToList();
            var mostCommonValue = nonNullValues
                .GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;

            // Extract numeric values for statistical calculations
            double[] numericValues = nonNullValues
                .Where(v => v is double || v is float || v is int || v is long || v is decimal)
                .Select(Convert.ToDouble)
                .ToArray();

            // Calculate mean, standard deviation, and skewness for numeric columns
            var mean = numericValues.Length > 0 ? numericValues.Average() : double.NaN;
            var standardDeviation = numericValues.Length > 0 ? CalculateStandardDeviation(numericValues) : double.NaN;
            var skewness = numericValues.Length > 0 ? CalculateSkewness(numericValues) : double.NaN;

            // Create a ColumnInfo instance for each column
            var columnInfo = new ColumnInfo
            {
                Column = column.Name,
                Type = column.DataType.ToString(),
                NonNullCount = nonNullValues.Count,
                NullCount = values.Count(v => v == null),
                UniqueCount = uniqueValues.Count,
                MostCommonValue = mostCommonValue,
                Skewness = skewness,
                Min = numericValues.Length > 0 ? numericValues.Min() : null,
                Max = numericValues.Length > 0 ? numericValues.Max() : null,
                Mean = mean,
                StandardDeviation = standardDeviation
            };

            // Perform detailed analysis directly within this context
            AnalyzeColumn(columnInfo, nonNullValues, numericValues);

            return columnInfo;
        }).ToList();

        return columnInfos;
    }

    private static void AnalyzeColumn(ColumnInfo column, List<object> nonNullValues, double[] numericValues)
    {
        bool isNumeric = numericValues.Length > 0;
        bool isCategorical = column.Type == "System.String";

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

    // Utility function to calculate standard deviation
    private static double CalculateStandardDeviation(double[] values)
    {
        if (values.Length == 0) return double.NaN;

        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Length;
        return Math.Sqrt(variance);
    }

    // Utility function to calculate skewness
    private static double CalculateSkewness(double[] values)
    {
        if (values.Length == 0) return double.NaN;

        var mean = values.Average();
        var stdDev = CalculateStandardDeviation(values);
        if (stdDev == 0) return 0;

        var skewness = values.Sum(v => Math.Pow((v - mean) / stdDev, 3)) / values.Length;
        return skewness;
    }
    public static List<BivariateAnalysis> GetBivariateAnalysis(this DataFrame dataFrame)
    {
        var bivariateAnalyses = new List<BivariateAnalysis>();

        // Generate all unique pairs of columns
        var columnPairs = from i in Enumerable.Range(0, dataFrame.Columns.Count)
                          from j in Enumerable.Range(i + 1, dataFrame.Columns.Count - i - 1)
                          select (dataFrame.Columns[i], dataFrame.Columns[j]);

        foreach (var (column1, column2) in columnPairs)
        {
            var analysis = new BivariateAnalysis
            {
                Column1 = column1.Name,
                Column2 = column2.Name
            };

            // Perform bivariate analysis on the pair
            AnalyzeColumnPair(analysis, column1, column2);

            bivariateAnalyses.Add(analysis);
        }

        return bivariateAnalyses;
    }

    private static void AnalyzeColumnPair(BivariateAnalysis analysis, DataFrameColumn column1, DataFrameColumn column2)
    {
        // Convert columns to object arrays for analysis
        var values1 = column1.Cast<object>().ToArray();
        var values2 = column2.Cast<object>().ToArray();

        var uniqueValues1 = values1.Where(v => v != null).Distinct().ToList();
        var uniqueValues2 = values2.Where(v => v != null).Distinct().ToList();

        // Handle numeric analysis if both columns are numeric
        bool column1IsNumeric = IsNumericType(column1.DataType);
        bool column2IsNumeric = IsNumericType(column2.DataType);

        // Analyze numeric vs. numeric columns
        if (column1IsNumeric && column2IsNumeric)
        {
            double[] numericValues1 = uniqueValues1
                .Where(v => v != null)
                .Select(Convert.ToDouble)
                .ToArray();

            double[] numericValues2 = uniqueValues2
                .Where(v => v != null)
                .Select(Convert.ToDouble)
                .ToArray();

            // Calculate correlation
            if (numericValues1.Length == numericValues2.Length && numericValues1.Length > 0)
            {
                var correlation = CalculateCorrelation(numericValues1, numericValues2);
                analysis.Observations.Add($"Correlation between '{analysis.Column1}' and '{analysis.Column2}' is {correlation:F2}.");

                // Visualization recommendations based on correlation and unique values
                if (Math.Abs(correlation) > 0.7)
                {
                    analysis.Observations.Add("Strong correlation detected. Consider further analysis or feature engineering.");
                    analysis.VisualizationRecommendations.Add("Scatter plot with trend line");
                    analysis.InsightScore = Math.Min(Math.Abs(correlation) * 1.5, 1.0);
                }
                else if (Math.Abs(correlation) < 0.3)
                {
                    analysis.Observations.Add("Weak correlation detected. Variables may be mostly independent.");
                    analysis.VisualizationRecommendations.Add("Scatter plot without trend line, explore other relationships");
                    analysis.InsightScore = Math.Max(0.1, Math.Abs(correlation));
                }
                else
                {
                    analysis.Observations.Add("Moderate correlation detected.");
                    analysis.VisualizationRecommendations.Add("Scatter plot");
                    analysis.InsightScore = Math.Abs(correlation);
                }
            }
            else
            {
                analysis.Observations.Add("Unable to calculate correlation due to mismatched or insufficient data.");
                analysis.InsightScore = 0;
            }
        }

        // Handle numeric vs. categorical combinations
        else if (column1IsNumeric && column2.DataType == typeof(string))
        {
            analysis.Observations.Add($"Analyzing '{analysis.Column1}' with '{analysis.Column2}' suggests grouping numeric data by categories.");
            RecommendVisualizationsForNumericVsCategorical(analysis, uniqueValues2);
            analysis.InsightScore = CalculateInsightScore(uniqueValues2.Count);
        }
        else if (column1.DataType == typeof(string) && column2IsNumeric)
        {
            analysis.Observations.Add($"Analyzing '{analysis.Column2}' with '{analysis.Column1}' suggests grouping numeric data by categories.");
            RecommendVisualizationsForNumericVsCategorical(analysis, uniqueValues1);
            analysis.InsightScore = CalculateInsightScore(uniqueValues1.Count);
        }

        // Handle categorical vs. categorical
        else if (column1.DataType == typeof(string) && column2.DataType == typeof(string))
        {
            analysis.Observations.Add($"Both '{analysis.Column1}' and '{analysis.Column2}' are categorical. Consider using cross-tabulation or chi-squared tests.");
            RecommendVisualizationsForCategoricalVsCategorical(analysis, uniqueValues1, uniqueValues2);
            analysis.InsightScore = CalculateInsightScore(Math.Min(uniqueValues1.Count, uniqueValues2.Count));
        }
        else
        {
            analysis.Observations.Add($"Analysis between '{analysis.Column1}' and '{analysis.Column2}' did not fit predefined types.");
            analysis.InsightScore = 0.2;
        }
    }

    private static void RecommendVisualizationsForNumericVsCategorical(BivariateAnalysis analysis, List<object> uniqueCategoricalValues)
    {
        if (uniqueCategoricalValues.Count < 10)
        {
            analysis.VisualizationRecommendations.Add("Box plot or violin plot grouped by categorical values");
        }
        else
        {
            analysis.VisualizationRecommendations.Add("Bar plot of means or medians grouped by categories");
        }
    }

    private static void RecommendVisualizationsForCategoricalVsCategorical(BivariateAnalysis analysis, List<object> uniqueValues1, List<object> uniqueValues2)
    {
        if (uniqueValues1.Count <= 10 && uniqueValues2.Count <= 10)
        {
            analysis.VisualizationRecommendations.Add("Heatmap or clustered bar chart");
        }
        else
        {
            analysis.VisualizationRecommendations.Add("Stacked bar chart or mosaic plot for large numbers of unique values");
        }
    }

    // Calculate an insight score based on the number of unique values
    private static double CalculateInsightScore(int uniqueValueCount)
    {
        if (uniqueValueCount <= 5)
            return 0.9; // High potential for insights
        else if (uniqueValueCount <= 20)
            return 0.7; // Moderate potential
        else
            return 0.4; // Lower potential due to higher variability
    }

    // Utility function to check if a type is numeric
    private static bool IsNumericType(Type type)
    {
        return type == typeof(double) || type == typeof(float) || type == typeof(int) || type == typeof(long) || type == typeof(decimal);
    }

    // Utility function to calculate correlation between two numeric arrays
    private static double CalculateCorrelation(double[] values1, double[] values2)
    {
        var mean1 = values1.Average();
        var mean2 = values2.Average();

        var sumProduct = values1.Zip(values2, (v1, v2) => (v1 - mean1) * (v2 - mean2)).Sum();
        var sumSquare1 = values1.Sum(v => Math.Pow(v - mean1, 2));
        var sumSquare2 = values2.Sum(v => Math.Pow(v - mean2, 2));

        var correlation = sumProduct / Math.Sqrt(sumSquare1 * sumSquare2);
        return correlation;
    }

}
