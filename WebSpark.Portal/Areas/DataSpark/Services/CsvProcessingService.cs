using Microsoft.Data.Analysis;
using System.Globalization;
using System.Text;
using WebSpark.Portal.Areas.DataSpark.Models;

namespace WebSpark.Portal.Areas.DataSpark.Services
{
    public class CsvProcessingService
    {
        public CsvViewModel ProcessCsvFast(string filePath)
        {
            var model = new CsvViewModel
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath)
            };

            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Load the DataFrame with automatic column type detection
                var dataFrame = DataFrame.LoadCsv(
                    stream,
                    separator: ',',
                    header: true,
                    encoding: Encoding.UTF8,
                    cultureInfo: CultureInfo.InvariantCulture
                );

                model.RowCount = dataFrame.Rows.Count;
                model.ColumnCount = dataFrame.Columns.Count;
                model.ColumnDetails = dataFrame.GetColumnAnalysis();
                model.BivariateAnalyses = dataFrame.GetBivariateAnalysis();
                model.Info = dataFrame.Info();
                model.Description = dataFrame.Description();
                model.Head = dataFrame.Head(5);


            }
            catch (Exception ex)
            {
                model.Message = $"Error processing file: {ex.Message}";
                throw; // Rethrow the exception to allow fallback
            }
            return model;
        }

        public CsvViewModel ProcessCsvSafe(string filePath)
        {
            var model = new CsvViewModel
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath)
            };

            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Load all columns as strings to avoid automatic parsing issues
                var dataFrame = DataFrame.LoadCsv(
                    stream,
                    separator: ',',
                    header: true,
                    dataTypes: Enumerable.Repeat(typeof(string), CsvProcessingUtils.GetColumnCount(filePath)).ToArray(),
                    encoding: Encoding.UTF8,
                    cultureInfo: CultureInfo.InvariantCulture
                );

                model.RowCount = dataFrame.Rows.Count;
                model.ColumnCount = dataFrame.Columns.Count;
                model.ColumnDetails = dataFrame.GetColumnAnalysis();
                model.BivariateAnalyses = dataFrame.GetBivariateAnalysis();
                model.Info = dataFrame.Info();
                model.Description = dataFrame.Description();
                model.Head = dataFrame.Head(5);


            }
            catch (Exception ex)
            {
                model.Message = $"Error processing file: {ex.Message}";
            }
            return model;
        }

        public CsvViewModel ProcessCsvWithFallback(string filePath)
        {
            try
            {
                // Attempt to process the CSV using the fast method
                return ProcessCsvFast(filePath);
            }
            catch
            {
                // If an error occurs, fall back to the safer method
                return ProcessCsvSafe(filePath);
            }
        }
    }
}