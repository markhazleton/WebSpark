using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Globalization;

namespace DataSpark.Web.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class FilesController : Controller
{
    private readonly IWebHostEnvironment _env;

    public FilesController(IWebHostEnvironment env)
    {
        _env = env;
    }

    // Endpoint to list all CSV files with detailed EDA
    [HttpGet("list")]
    public IActionResult ListFilesWithEDA()
    {
        try
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "files");
            if (!Directory.Exists(uploadPath))
            {
                return Ok(new { message = "No files directory found.", files = new List<object>() });
            }

            var files = Directory.GetFiles(uploadPath, "*.csv")
                                 .Select(filePath => PerformFileLevelEDA(filePath))
                                 .ToList();

            if (!files.Any())
            {
                return Ok(new { message = "No CSV files found.", files = new List<object>() });
            }

            return Ok(new { message = "Files retrieved successfully.", files });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error retrieving files: {ex.Message}" });
        }
    }

    private object PerformFileLevelEDA(string filePath)
    {
        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            var records = csv.GetRecords<dynamic>().ToList();

            if (!records.Any())
            {
                return new
                {
                    FileName = Path.GetFileName(filePath),
                    NumberOfRows = 0,
                    NumberOfColumns = 0,
                    FileSizeKB = new FileInfo(filePath).Length / 1024.0,
                    Columns = new List<object>(),
                    NullPercentage = 0,
                    DuplicateRows = 0
                };
            }

            var headers = ((IDictionary<string, object>)records[0]).Keys.ToList();
            var columnDetails = PerformColumnLevelEDA(records, headers);

            return new
            {
                FileName = Path.GetFileName(filePath),
                NumberOfRows = records.Count,
                NumberOfColumns = headers.Count,
                FileSizeKB = new FileInfo(filePath).Length / 1024.0,
                Columns = columnDetails,
                NullPercentage = columnDetails.Sum(c => ((dynamic)c).NullCount) / (double)(records.Count * headers.Count) * 100,
                DuplicateRows = CountDuplicateRows(records)
            };
        }
        catch
        {
            return new
            {
                FileName = Path.GetFileName(filePath),
                NumberOfRows = 0,
                NumberOfColumns = 0,
                FileSizeKB = new FileInfo(filePath).Length / 1024.0,
                Columns = new List<object>(),
                NullPercentage = 0,
                DuplicateRows = 0
            };
        }
    }

    private List<object> PerformColumnLevelEDA(List<dynamic> records, List<string> headers)
    {
        var columnDetails = new List<object>();

        foreach (var header in headers)
        {
            var columnData = records.Select(r => ((IDictionary<string, object>)r)[header]?.ToString()).ToList();

            // Determine data type
            bool isNumeric = columnData.All(value => double.TryParse(value, out _) || string.IsNullOrEmpty(value));
            string dataType = isNumeric ? "Numeric" : "Categorical";

            // Calculate unique count
            var uniqueValues = columnData.Distinct().Where(v => !string.IsNullOrEmpty(v)).ToList();
            int uniqueCount = uniqueValues.Count;

            // Determine most common value
            var mostCommonValue = columnData
                .Where(v => !string.IsNullOrEmpty(v))
                .GroupBy(v => v)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;

            // Null count
            int nullCount = columnData.Count(v => string.IsNullOrEmpty(v));

            // Additional statistics for numeric data
            double? minValue = null, maxValue = null, mean = null, stdDev = null;
            if (isNumeric)
            {
                var numericValues = columnData
                    .Where(v => double.TryParse(v, out _))
                    .Select(v => double.Parse(v))
                    .ToList();

                if (numericValues.Any())
                {
                    minValue = numericValues.Min();
                    maxValue = numericValues.Max();
                    mean = numericValues.Average();
                    stdDev = Math.Sqrt(numericValues.Select(v => Math.Pow(v - mean.Value, 2)).Average());
                }
            }

            // Add EDA results for the column
            columnDetails.Add(new
            {
                ColumnName = header,
                DataType = dataType,
                UniqueCount = uniqueCount,
                MostCommonValue = mostCommonValue,
                NullCount = nullCount,
                MinValue = minValue,
                MaxValue = maxValue,
                Mean = mean,
                StdDev = stdDev
            });
        }

        return columnDetails;
    }

    private int CountDuplicateRows(List<dynamic> records)
    {
        return records
            .GroupBy(r => string.Join(",", ((IDictionary<string, object>)r).Values))
            .Where(g => g.Count() > 1)
            .Sum(g => g.Count() - 1);
    }


    [HttpGet("analyze")]
    public IActionResult AnalyzeFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("File name is required.");
        }

        var filePath = Path.Combine(_env.WebRootPath, "files", fileName);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

            // Get headers
            csv.Read();
            csv.ReadHeader();
            var headers = csv.HeaderRecord;

            return Ok(headers); // Return column names for the user to choose
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred while analyzing the file: {ex.Message}");
        }
    }

    [HttpPost("train")]
    public IActionResult TrainModel(string fileName, string targetColumn)
    {
        if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(targetColumn))
        {
            return BadRequest("File name and target column are required.");
        }

        var filePath = Path.Combine(_env.WebRootPath, "files", fileName);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        try
        {
            // Load data from the CSV file
            var mlContext = new MLContext();
            var dataView = mlContext.Data.LoadFromTextFile(filePath, new TextLoader.Options
            {
                Separators = new[] { ',' },
                HasHeader = true,
                Columns = AnalyzeColumns(filePath, targetColumn)
            });

            // Split data into training and test sets
            var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // Define the data preparation and training pipeline
            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(targetColumn, "KeyLabel")
                .Append(mlContext.Transforms.Concatenate("Features", GetFeatureColumns(filePath, targetColumn)))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("KeyLabel", "Features"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(targetColumn, "KeyLabel"));

            // Train the model
            var model = pipeline.Fit(splitData.TrainSet);

            // Evaluate the model
            var metrics = mlContext.MulticlassClassification.Evaluate(
                data: splitData.TestSet,
                labelColumnName: "KeyLabel",
                predictedLabelColumnName: targetColumn,
                scoreColumnName: "Score");

            return Ok(new
            {
                Accuracy = metrics.MacroAccuracy,
                LogLoss = metrics.LogLoss,
                LogLossReduction = metrics.LogLossReduction
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred while training the model: {ex.Message}");
        }
    }

    private TextLoader.Column[] AnalyzeColumns(string filePath, string targetColumn)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;

        var columns = headers.Select((header, index) =>
        {
            if (header == targetColumn)
            {
                return new TextLoader.Column(header, DataKind.String, index);
            }
            return new TextLoader.Column(header, DataKind.Single, index);
        }).ToArray();

        return columns;
    }

    private string[] GetFeatureColumns(string filePath, string targetColumn)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        csv.Read();
        csv.ReadHeader();
        return csv.HeaderRecord.Where(h => h != targetColumn).ToArray();
    }
}

public class ModelInput
{
    public string Label { get; set; }
    public float[] NumericFeatures { get; set; }
}