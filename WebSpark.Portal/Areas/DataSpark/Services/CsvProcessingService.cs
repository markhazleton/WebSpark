using Microsoft.Data.Analysis;
using System.Globalization;
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
}
