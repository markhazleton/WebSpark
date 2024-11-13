﻿using Microsoft.Data.Analysis;
using System.Data;
using System.Globalization;
using System.Text.Json;
using WebSpark.Portal.Areas.DataSpark.Models;

namespace WebSpark.Portal.Areas.DataSpark.Services;

/// <summary>
/// Service for processing CSV files.
/// </summary>
public class CsvProcessingService
{
    /// <summary>
    /// Process the CSV file with fallback to the safe method if an error occurs.
    /// </summary>
    /// <param name="filePath">The path of the CSV file.</param>
    /// <returns>The processed CsvViewModel.</returns>
    public CsvViewModel ProcessCsvWithFallback(string filePath)
    {
        try
        {
            // Attempt to process the CSV using the fast method
            return ProcessCsv(filePath, useSafeMethod: false);
        }
        catch
        {
            // If an error occurs, fall back to the safer method
            return ProcessCsv(filePath, useSafeMethod: true);
        }
    }

    /// <summary>
    /// Process the CSV file with an option to use a safe method.
    /// </summary>
    /// <param name="filePath">The path of the CSV file.</param>
    /// <param name="useSafeMethod">If true, loads all columns as strings; otherwise, uses type detection.</param>
    /// <returns>The processed CsvViewModel.</returns>
    private static CsvViewModel ProcessCsv(string filePath, bool useSafeMethod)
    {
        var model = new CsvViewModel
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath)
        };

        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            DataFrame dataFrame;
            if (useSafeMethod)
            {
                // Load all columns as strings to avoid automatic parsing issues
                dataFrame = DataFrame.LoadCsv(
                    stream,
                    separator: ',',
                    header: true,
                    dataTypes: Enumerable.Repeat(typeof(string), CsvProcessingUtils.GetColumnCount(filePath)).ToArray(),
                    encoding: Encoding.UTF8,
                    cultureInfo: CultureInfo.InvariantCulture
                );
            }
            else
            {
                // Load the DataFrame with automatic column type detection
                dataFrame = DataFrame.LoadCsv(
                    stream,
                    separator: ',',
                    header: true,
                    encoding: Encoding.UTF8,
                    cultureInfo: CultureInfo.InvariantCulture
                );
            }
            PopulateCsvViewModel(model, dataFrame);
        }
        catch (Exception ex)
        {
            model.Message = $"Error processing file: {ex.Message}";
            if (!useSafeMethod) throw; // Rethrow the exception only if not using the safe method
        }
        return model;
    }
    private double CalculateMedian(double[] data)
    {
        Array.Sort(data);
        int count = data.Length;
        if (count % 2 == 0)
        {
            return (data[count / 2 - 1] + data[count / 2]) / 2.0; // Average of middle two
        }
        else
        {
            return data[count / 2]; // Middle value
        }
    }

    private double CalculateQuantile(double[] data, double quantile)
    {
        Array.Sort(data);
        if (quantile < 0.0 || quantile > 1.0) throw new ArgumentOutOfRangeException(nameof(quantile));

        double position = (data.Length - 1) * quantile;
        int lowerIndex = (int)position;
        double fraction = position - lowerIndex;

        if (lowerIndex + 1 < data.Length)
        {
            return data[lowerIndex] + fraction * (data[lowerIndex + 1] - data[lowerIndex]);
        }
        return data[lowerIndex]; // If position is exactly an index
    }
    public string GetScottPlotSvg(string columnName, DataTable dataTable)
    {
        var columnData = dataTable.AsEnumerable().Select(row => row[columnName].ToString()).ToList();
        var isNumeric = columnData.All(value => double.TryParse(value, out _));

        var plt = new ScottPlot.Plot();
        if (isNumeric)
        {
            var data = dataTable.AsEnumerable()
                                     .Select(row => Convert.ToDouble(row[columnName]))
                                     .ToArray();

            // Calculate statistics for the box plot
            double min = data.Min();
            double max = data.Max();
            double median = CalculateMedian(data);
            double q1 = CalculateQuantile(data, 0.25);
            double q3 = CalculateQuantile(data, 0.75);

            // Create a box plot using the calculated statistics
            ScottPlot.Box box = new()
            {
                Position = 1,  // Set the position for the box plot
                BoxMin = q1,
                BoxMax = q3,
                WhiskerMin = min,
                WhiskerMax = max,
                BoxMiddle = median,
            };

            plt.Add.Box(box);
            plt.Title($"Box Plot of {columnName}");
            plt.XLabel(columnName);
            plt.YLabel("Values");
        }
        else
        {
            // Handle categorical data with a frequency count plot
            var valueCounts = columnData.GroupBy(x => x)
                .ToDictionary(g => g.Key ?? string.Empty, g => g.Count());

            // Sort the dictionary by counts in descending order
            var sortedValueCounts = valueCounts.OrderByDescending(kvp => kvp.Value).ToList();
            var categories = sortedValueCounts.Select(kvp => kvp.Key).ToArray();
            var counts = sortedValueCounts.Select(kvp => (double)kvp.Value).ToArray();

            var bars = categories.Select((category, index) => new ScottPlot.Bar
            {
                Position = index,
                Value = counts[index],
                Label = category
            }).ToArray();

            plt.Add.Bars(bars);
            plt.Title($"Frequency Count of {columnName}");
            plt.XLabel("Category");
            plt.YLabel("Count");
        }
        return plt.GetSvgXml(600, 400);
    }

    public string GetScottBarPlotSvg(string columnName, DataTable dataTable)
    {
        var columnData = dataTable.AsEnumerable().Select(row => row[columnName].ToString()).ToList();
        var isNumeric = columnData.All(value => double.TryParse(value, out _));

        var plt = new ScottPlot.Plot();
        if (isNumeric)
        {
            var data = dataTable.AsEnumerable()
                                .Select(row => Convert.ToDouble(row[columnName]))
                                .ToArray();

            plt.Add.Bars(data);
            plt.Title($"Univariate Analysis of {columnName}");
            plt.XLabel(columnName);
            plt.YLabel("Frequency");
        }
        else
        {
            // Handle categorical data
            var valueCounts = columnData.GroupBy(x => x)
                .ToDictionary(g => g.Key ?? string.Empty, g => g.Count());

            // Sort the dictionary by counts in descending order
            var sortedValueCounts = valueCounts.OrderByDescending(kvp => kvp.Value).ToList();
            var categories = sortedValueCounts.Select(kvp => kvp.Key).ToArray();
            var counts = sortedValueCounts.Select(kvp => (double)kvp.Value).ToArray();

            var bars = categories.Select((category, index) => new ScottPlot.Bar
            {
                Position = index,
                Value = counts[index],
                Label = category
            }).ToArray();

            plt.Add.Bars(bars);
            plt.Title($"Frequency Count of {columnName}");
            plt.XLabel("Category");
            plt.YLabel("Count");
        }
        return plt.GetSvgXml(600, 400);
    }

    /// <summary>
    /// Populates the CsvViewModel with details from the DataFrame.
    /// </summary>
    /// <param name="model">The CsvViewModel to populate.</param>
    /// <param name="dataFrame">The DataFrame containing the CSV data.</param>
    private static void PopulateCsvViewModel(CsvViewModel model, DataFrame dataFrame)
    {
        model.RowCount = dataFrame.Rows.Count;
        model.ColumnCount = dataFrame.Columns.Count;
        model.ColumnDetails = dataFrame.GetUnivariateAnalysis();
        model.BivariateAnalyses = dataFrame.GetBivariateAnalysis();
        model.Info = dataFrame.Info();
        model.Description = dataFrame.Description();
        model.Head = dataFrame.Head(5);
    }

    /// <summary>
    /// Processes the CSV file and returns the contents as a JSON string.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <param name="useSafeMethod">If true, loads all columns as strings to avoid parsing issues; otherwise, type detection is used.</param>
    /// <returns>A JSON string representing the contents of the CSV file.</returns>
    public string ProcessCsvToJson(string filePath, bool useSafeMethod)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            DataFrame dataFrame;

            if (useSafeMethod)
            {
                // Load all columns as strings
                dataFrame = DataFrame.LoadCsv(
                    stream,
                    separator: ',',
                    header: true,
                    dataTypes: Enumerable.Repeat(typeof(string), CsvProcessingUtils.GetColumnCount(filePath)).ToArray(),
                    encoding: Encoding.UTF8,
                    cultureInfo: CultureInfo.InvariantCulture
                );
            }
            else
            {
                // Load the DataFrame with automatic type detection
                dataFrame = DataFrame.LoadCsv(
                    stream,
                    separator: ',',
                    header: true,
                    encoding: Encoding.UTF8,
                    cultureInfo: CultureInfo.InvariantCulture
                );
            }

            // Convert the DataFrame to a JSON string
            return DataFrameToJson(dataFrame);
        }
        catch (Exception ex)
        {
            // Handle errors and return the error message as JSON
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Converts a DataFrame into a JSON string.
    /// </summary>
    /// <param name="dataFrame">The DataFrame to convert to JSON.</param>
    /// <returns>A JSON string representing the DataFrame contents.</returns>
    private static string DataFrameToJson(DataFrame dataFrame)
    {
        var rows = new List<Dictionary<string, object>>();

        // Iterate through each row in the DataFrame
        foreach (var row in dataFrame.Rows)
        {
            var rowDict = new Dictionary<string, object>();

            for (int i = 0; i < dataFrame.Columns.Count; i++)
            {
                var columnName = dataFrame.Columns[i].Name;
                rowDict[columnName] = row[i]; // Add each column value to the dictionary
            }

            rows.Add(rowDict);
        }

        // Serialize the list of rows to a JSON string
        return JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true });
    }

}
