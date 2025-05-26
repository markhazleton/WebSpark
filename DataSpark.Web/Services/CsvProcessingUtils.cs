using DataSpark.Web.Models;
using Microsoft.Data.Analysis;
using System.Globalization;

namespace DataSpark.Web.Services;

public static class CsvProcessingUtils
{
    public static double CalculateSkewness(double[] values)
    {
        if (values.Length < 3) return double.NaN;
        double mean = values.Average();
        double standardDeviation = CalculateStandardDeviation(values);
        double skewness = values.Sum(v => Math.Pow((v - mean) / standardDeviation, 3)) / values.Length;
        return skewness;
    }

    public static double CalculateStandardDeviation(double[] values)
    {
        if (values.Length == 0) return double.NaN;
        double mean = values.Average();
        double sum = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sum / values.Length);
    }

    public static int GetColumnCount(string filePath)
    {
        // A utility method to determine the number of columns, perhaps by reading the first line
        using var reader = new StreamReader(filePath);
        var headerLine = reader.ReadLine();
        return headerLine?.Split(',').Length ?? 0;
    }
    public static bool IsDateColumn(DataFrameColumn column, out string? detectedFormat)
    {
        var dateFormats = new[]
        {
            "dd-MM-yyyy HH:mm", "MM-dd-yyyy HH:mm", "yyyy-MM-dd HH:mm",
            "dd/MM/yyyy HH:mm", "MM/dd/yyyy HH:mm", "yyyy/MM/dd HH:mm",
            "dd-MM-yyyy", "MM-dd-yyyy", "yyyy-MM-dd",
            "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/MM/dd"
        };

        detectedFormat = null;
        int sampleSize = (int)Math.Min(column.Length, 5);
        for (int i = 0; i < sampleSize; i++)
        {
            if (column[i] is string dateStr)
            {
                dateStr = dateStr.Trim();
                foreach (var format in dateFormats)
                {
                    if (DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    {
                        detectedFormat = format;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public static void ParseAndConvertDatesInColumn(DataFrame dataFrame, DataFrameColumn column, string dateFormat, ColumnInfo columnInfo)
    {
        var dateTimeColumn = new PrimitiveDataFrameColumn<DateTime>(column.Name, column.Length);
        bool conversionSuccessful = true;

        for (int i = 0; i < column.Length; i++)
        {
            if (column[i] is string dateStr)
            {
                dateStr = dateStr.Trim();
                try
                {
                    if (DateTime.TryParseExact(dateStr, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue))
                    {
                        dateTimeColumn[i] = dateValue; // Replace string with DateTime object
                    }
                    else
                    {
                        dateTimeColumn[i] = null; // Set to null if parsing fails
                        columnInfo.Errors.Add($"Failed to parse date: {dateStr} at row {i + 1}");
                        conversionSuccessful = false;
                    }
                }
                catch (Exception ex)
                {
                    dateTimeColumn[i] = null; // Set to null on error
                    columnInfo.Errors.Add($"Error parsing date '{dateStr}' at row {i + 1}: {ex.Message}");
                    conversionSuccessful = false;
                }
            }
        }

        // Replace the original string column with the DateTime column if conversion was mostly successful
        if (conversionSuccessful)
        {
            dataFrame.Columns.Remove(column);
            dataFrame.Columns.Insert(dataFrame.Columns.IndexOf(column), dateTimeColumn);
        }
        else
        {
            columnInfo.Errors.Add($"Conversion to DateTime failed for column '{column.Name}'. Kept as string.");
        }
    }
}
