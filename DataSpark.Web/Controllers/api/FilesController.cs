using CsvHelper;
using CsvHelper.Configuration;
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
                    .Where(v => !string.IsNullOrEmpty(v) && double.TryParse(v, out _))
                    .Select(v => double.Parse(v!))
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
    public IActionResult TrainModel([FromForm] string fileName, [FromForm] string targetColumn)
    {
        if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(targetColumn))
        {
            return BadRequest(new { error = "File name and target column are required.", fileName, targetColumn });
        }

        var filePath = Path.Combine(_env.WebRootPath, "files", fileName);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { error = "File not found.", fileName, filePath });
        }

        try
        {
            // Check if target column exists and count rows
            string[] headers = Array.Empty<string>();
            int rowCount = 0;
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                if (!csv.Read() || !csv.ReadHeader())
                {
                    return BadRequest(new { error = "CSV file is empty or malformed.", fileName, filePath });
                }
                headers = csv.HeaderRecord ?? Array.Empty<string>();
                if (headers.Length == 0 || !headers.Contains(targetColumn))
                {
                    return BadRequest(new { error = $"Target column '{targetColumn}' not found in file.", headers, fileName, filePath });
                }
                while (csv.Read()) rowCount++;
            }
            if (rowCount < 10)
            {
                return BadRequest(new { error = "Not enough rows in the file for training (minimum 10 required).", rowCount, fileName, filePath });
            }

            // Additional validation and error checks
            // 1. Sanitize file name to prevent directory traversal
            if (fileName.Contains("..") || fileName.Contains(":") || fileName.Contains("/"))
            {
                return BadRequest(new { error = "Invalid file name.", fileName });
            }

            // 2. Check for duplicate or empty column names
            if (headers.Distinct().Count() != headers.Length)
            {
                return BadRequest(new { error = "Duplicate column names found in header.", headers, fileName });
            }
            if (headers.Any(h => string.IsNullOrWhiteSpace(h)))
            {
                return BadRequest(new { error = "Empty or whitespace column name found in header.", headers, fileName });
            }

            // 3. Check for malformed rows (inconsistent column count)
            int malformedRowCount = 0;
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    if (csv.Parser.Count != headers.Length)
                        malformedRowCount++;
                }
            }
            if (malformedRowCount > 0)
            {
                return BadRequest(new { error = $"Malformed rows found: {malformedRowCount} rows have inconsistent column count.", malformedRowCount, fileName });
            }

            // 4. Check for all-missing or constant columns
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.Read();
                csv.ReadHeader();
                var allRows = new List<string[]>();
                while (csv.Read())
                {
                    // Fix: ensure non-null arrays and values for nullability
                    allRows.Add(headers.Select(h => csv.GetField(h) ?? string.Empty).ToArray());
                }
                foreach (var h in headers)
                {
                    var colVals = allRows.Select(r => r[Array.IndexOf(headers, h)]).ToList();
                    if (colVals.All(string.IsNullOrEmpty))
                    {
                        return BadRequest(new { error = $"Column '{h}' has all missing values.", fileName });
                    }
                    if (colVals.Distinct().Count() == 1)
                    {
                        return BadRequest(new { error = $"Column '{h}' is constant (only one unique value).", fileName });
                    }
                }
            }

            // 5. Check if target column has only one unique value
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.Read();
                csv.ReadHeader();
                var targetVals = new HashSet<string>();
                while (csv.Read())
                {
                    targetVals.Add(csv.GetField(targetColumn) ?? string.Empty);
                }
                if (targetVals.Count <= 1)
                {
                    return BadRequest(new { error = $"Target column '{targetColumn}' has only one unique value.", fileName });
                }
            }

            // 6. Check for at least one valid feature column
            var featureColumns = headers.Where(h => h != targetColumn).ToArray();
            if (featureColumns.Length == 0)
            {
                return BadRequest(new { error = "No feature columns available after excluding the target column.", fileName });
            }

            var mlContext = new MLContext();
            var dataView = mlContext.Data.LoadFromTextFile(filePath, new TextLoader.Options
            {
                Separators = new[] { ',' },
                HasHeader = true,
                Columns = AnalyzeColumns(filePath, targetColumn)
            });

            // Split data into training and test sets
            var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            if (splitData.TrainSet == null || splitData.TestSet == null)
            {
                return BadRequest(new { error = "Failed to split data into training and test sets.", fileName, filePath });
            }

            // Detect if the target column is numeric or categorical
            bool isTargetNumeric = true;
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var value = csv.GetField(targetColumn);
                    if (!string.IsNullOrEmpty(value) && !double.TryParse(value, out _))
                    {
                        isTargetNumeric = false;
                        break;
                    }
                }
            }

            IEstimator<ITransformer> pipeline;
            if (isTargetNumeric)
            {
                // Regression pipeline
                pipeline = mlContext.Transforms.Concatenate("Features", GetFeatureColumns(filePath, targetColumn))
                    .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: targetColumn, featureColumnName: "Features"));
            }
            else
            {
                // Classification pipeline
                pipeline = mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: targetColumn, outputColumnName: "Label")
                    .Append(mlContext.Transforms.Concatenate("Features", GetFeatureColumns(filePath, targetColumn)))
                    .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features"))
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: targetColumn, inputColumnName: "Label"));
            }

            // Train the model
            ITransformer model;
            try
            {
                model = pipeline.Fit(splitData.TrainSet);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Model training failed: {ex.Message}", details = ex.ToString(), fileName, filePath });
            }

            // Evaluate the model
            object metricsResult;
            try
            {
                if (isTargetNumeric)
                {
                    var metrics = mlContext.Regression.Evaluate(splitData.TestSet, labelColumnName: targetColumn, scoreColumnName: "Score");
                    metricsResult = new
                    {
                        metrics.RSquared,
                        metrics.MeanAbsoluteError,
                        metrics.MeanSquaredError,
                        metrics.RootMeanSquaredError
                    };
                }
                else
                {
                    var metrics = mlContext.MulticlassClassification.Evaluate(
                        data: splitData.TestSet,
                        labelColumnName: "Label",
                        predictedLabelColumnName: targetColumn,
                        scoreColumnName: "Score");
                    metricsResult = new
                    {
                        metrics.MacroAccuracy,
                        metrics.LogLoss,
                        metrics.LogLossReduction
                    };
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Model evaluation failed: {ex.Message}", details = ex.ToString(), fileName, filePath });
            }

            return Ok(metricsResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An unexpected error occurred while training the model: {ex.Message}", details = ex.ToString(), fileName, filePath });
        }
    }

    private TextLoader.Column[] AnalyzeColumns(string filePath, string targetColumn)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? Array.Empty<string>();
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
        var headers = csv.HeaderRecord ?? Array.Empty<string>();
        return headers.Where(h => h != targetColumn).ToArray();
    }

    [HttpGet("eda")]
    public IActionResult GetFileEDA([FromQuery] string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest(new { error = "File name is required." });
        var filePath = Path.Combine(_env.WebRootPath, "files", fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { error = "File not found.", fileName });
        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            var records = csv.GetRecords<dynamic>().ToList();
            if (!records.Any())
                return Ok(new { fileName, message = "File is empty." });
            var headers = ((IDictionary<string, object>)records[0]).Keys.ToList();
            // Head & Tail
            var head = records.Take(5).Select(r => headers.Select(h => ((IDictionary<string, object>)r)[h]?.ToString()).ToList()).ToList();
            var tail = records.Skip(Math.Max(0, records.Count - 5)).Select(r => headers.Select(h => ((IDictionary<string, object>)r)[h]?.ToString()).ToList()).ToList();
            // Summary stats per column
            var summary = new List<object>();
            var valueCounts = new List<object>();
            var missing = new List<object>();
            var numericColumns = new List<string>();
            var numericData = new Dictionary<string, List<double>>();
            foreach (var h in headers)
            {
                var colData = records.Select(r => ((IDictionary<string, object>)r)[h]?.ToString()).ToList();
                bool isNumeric = colData.All(v => double.TryParse(v, out _) || string.IsNullOrEmpty(v));
                if (isNumeric)
                {
                    var nums = colData
                        .Where(v => !string.IsNullOrEmpty(v) && double.TryParse(v, out _))
                        .Select(v => double.Parse(v!))
                        .ToList();
                    numericColumns.Add(h);
                    numericData[h] = nums;
                    summary.Add(new
                    {
                        column = h,
                        type = "Numeric",
                        count = nums.Count,
                        min = nums.Any() ? nums.Min() : (double?)null,
                        max = nums.Any() ? nums.Max() : (double?)null,
                        mean = nums.Any() ? nums.Average() : (double?)null,
                        std = nums.Count > 1 ? Math.Sqrt(nums.Select(x => Math.Pow(x - nums.Average(), 2)).Average()) : (double?)null,
                        median = nums.Any() ? (nums.OrderBy(x => x).ElementAt(nums.Count / 2)) : (double?)null
                    });
                }
                else
                {
                    summary.Add(new
                    {
                        column = h,
                        type = "Categorical",
                        count = colData.Count(v => !string.IsNullOrEmpty(v)),
                        unique = colData.Where(v => !string.IsNullOrEmpty(v)).Distinct().Count(),
                        top = colData.Where(v => !string.IsNullOrEmpty(v)).GroupBy(v => v).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key,
                        freq = colData.Where(v => !string.IsNullOrEmpty(v)).GroupBy(v => v).OrderByDescending(g => g.Count()).FirstOrDefault()?.Count() ?? 0
                    });
                }
                // Value counts (top 10)
                valueCounts.Add(new
                {
                    column = h,
                    counts = colData.Where(v => !string.IsNullOrEmpty(v)).GroupBy(v => v).OrderByDescending(g => g.Count()).Take(10).Select(g => new { value = g.Key, count = g.Count() }).ToList()
                });
                // Missing values
                missing.Add(new
                {
                    column = h,
                    missing = colData.Count(v => string.IsNullOrEmpty(v)),
                    percent = 100.0 * colData.Count(v => string.IsNullOrEmpty(v)) / colData.Count
                });
            }
            // Correlation matrix (numeric only)
            var correlation = new List<object>();
            if (numericColumns.Count > 1)
            {
                foreach (var col1 in numericColumns)
                {
                    var row = new Dictionary<string, object>();
                    foreach (var col2 in numericColumns)
                    {
                        var x = numericData[col1];
                        var y = numericData[col2];
                        double corr = double.NaN;
                        if (x.Count > 1 && y.Count > 1 && x.Count == y.Count)
                        {
                            double meanX = x.Average();
                            double meanY = y.Average();
                            double sumXY = x.Zip(y, (a, b) => (a - meanX) * (b - meanY)).Sum();
                            double sumX2 = x.Sum(a => Math.Pow(a - meanX, 2));
                            double sumY2 = y.Sum(b => Math.Pow(b - meanY, 2));
                            corr = (sumX2 == 0 || sumY2 == 0) ? double.NaN : sumXY / Math.Sqrt(sumX2 * sumY2);
                        }
                        row[col2] = corr;
                    }
                    correlation.Add(new { column = col1, values = row });
                }
            }
            return Ok(new
            {
                fileName,
                head,
                tail,
                summary,
                valueCounts,
                missing,
                correlation
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to analyze file: {ex.Message}", details = ex.ToString(), fileName });
        }
    }

    [HttpPost("bivariate")]
    public IActionResult BivariateAnalysis([FromForm] string fileName, [FromForm] string column1, [FromForm] string column2)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(column1) || string.IsNullOrWhiteSpace(column2))
            return BadRequest(new { error = "File name and both columns are required.", fileName, column1, column2 });
        var filePath = Path.Combine(_env.WebRootPath, "files", fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { error = "File not found.", fileName });
        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            var records = csv.GetRecords<dynamic>().ToList();
            if (!records.Any())
                return Ok(new { fileName, message = "File is empty." });
            var headers = ((IDictionary<string, object>)records[0]).Keys.ToList();
            if (!headers.Contains(column1) || !headers.Contains(column2))
                return BadRequest(new { error = "One or both columns not found in file.", headers, fileName });
            var col1Data = records.Select(r => ((IDictionary<string, object>)r)[column1]?.ToString()).ToList();
            var col2Data = records.Select(r => ((IDictionary<string, object>)r)[column2]?.ToString()).ToList();
            // Detect types
            bool col1Numeric = col1Data.All(v => double.TryParse(v, out _) || string.IsNullOrEmpty(v));
            bool col2Numeric = col2Data.All(v => double.TryParse(v, out _) || string.IsNullOrEmpty(v));
            bool col1Date = col1Data.All(v => DateTime.TryParse(v, out _) || string.IsNullOrEmpty(v));
            bool col2Date = col2Data.All(v => DateTime.TryParse(v, out _) || string.IsNullOrEmpty(v));
            // Prepare result object
            var result = new Dictionary<string, object?>
            {
                ["fileName"] = fileName,
                ["column1"] = column1,
                ["column2"] = column2,
                ["col1Type"] = col1Numeric ? "Numeric" : (col1Date ? "Date" : "Categorical"),
                ["col2Type"] = col2Numeric ? "Numeric" : (col2Date ? "Date" : "Categorical")
            };
            // Numeric-Numeric: scatter, correlation, regression
            if ((col1Numeric || col1Date) && (col2Numeric || col2Date))
            {
                var x = col1Data.Where(v => !string.IsNullOrEmpty(v) && double.TryParse(v, out _)).Select(v => double.Parse(v!)).ToList();
                var y = col2Data.Where(v => !string.IsNullOrEmpty(v) && double.TryParse(v, out _)).Select(v => double.Parse(v!)).ToList();
                int n = Math.Min(x.Count, y.Count);
                if (n > 1)
                {
                    x = x.Take(n).ToList();
                    y = y.Take(n).ToList();
                    double meanX = x.Average();
                    double meanY = y.Average();
                    double sumXY = x.Zip(y, (a, b) => (a - meanX) * (b - meanY)).Sum();
                    double sumX2 = x.Sum(a => Math.Pow(a - meanX, 2));
                    double sumY2 = y.Sum(b => Math.Pow(b - meanY, 2));
                    double corr = (sumX2 == 0 || sumY2 == 0) ? double.NaN : sumXY / Math.Sqrt(sumX2 * sumY2);
                    // Simple linear regression (y = a + bx)
                    double b = sumX2 == 0 ? 0 : sumXY / sumX2;
                    double a = meanY - b * meanX;
                    result["correlation"] = corr;
                    result["regression"] = new { intercept = a, slope = b };
                    result["scatter"] = x.Zip(y, (a1, b1) => new[] { a1, b1 }).ToList();
                }
            }
            // Categorical-Categorical: contingency table
            else if (!col1Numeric && !col2Numeric)
            {
                var table = col1Data.Zip(col2Data, (a, b) => new { a, b })
                    .GroupBy(x => x.a)
                    .ToDictionary(
                        g => g.Key ?? "",
                        g => g.GroupBy(x => x.b).ToDictionary(
                            gg => gg.Key ?? "",
                            gg => gg.Count()
                        )
                    );
                result["contingencyTable"] = table;
            }
            // Numeric-Categorical: boxplot/group stats
            else if ((col1Numeric && !col2Numeric) || (!col1Numeric && col2Numeric))
            {
                var numeric = col1Numeric ? col1Data : col2Data;
                var categorical = col1Numeric ? col2Data : col1Data;
                var groups = categorical.Zip(numeric, (cat, num) => new { cat, num })
                    .Where(x => !string.IsNullOrEmpty(x.num) && double.TryParse(x.num, out _))
                    .GroupBy(x => x.cat ?? "")
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var nums = g.Select(x => double.Parse(x.num!)).ToList();
                            return new
                            {
                                count = nums.Count,
                                min = nums.Any() ? nums.Min() : (double?)null,
                                max = nums.Any() ? nums.Max() : (double?)null,
                                mean = nums.Any() ? nums.Average() : (double?)null,
                                std = nums.Count > 1 ? Math.Sqrt(nums.Select(x => Math.Pow(x - nums.Average(), 2)).Average()) : (double?)null,
                                values = nums
                            };
                        }
                    );
                result["groupStats"] = groups;
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to perform bivariate analysis: {ex.Message}", details = ex.ToString(), fileName });
        }
    }
}

public class ModelInput
{
    public string? Label { get; set; }
    public float[]? NumericFeatures { get; set; }
}