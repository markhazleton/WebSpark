using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data;
using ScottPlot;
using System;
using WebSpark.Portal.Areas.DataSpark.Models;
using ScottPlot.Grids;

namespace WebSpark.Portal.Areas.DataSpark.Controllers
{
    public class UnivariateController : DataSparkBaseController
    {
        private readonly IConfiguration _configuration;

        public UnivariateController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new UnivariateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0 || Path.GetExtension(file.FileName)?.ToLower() != ".csv")
            {
                return View("Index", new UnivariateViewModel { Message = "Please upload a valid .csv file." });
            }

            try
            {
                // Get output folder from configuration
                var outputFolder = _configuration["CsvOutputFolder"];

                if (string.IsNullOrEmpty(outputFolder))
                {
                    return View("Index", new UnivariateViewModel { Message = "CSV output folder is not configured." });
                }

                // Ensure the directory exists
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                // Set file name and path
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(outputFolder, fileName);

                // Save the file to the specified path
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Load CSV and extract column names
                var dataTable = LoadCsv(filePath);
                var columns = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

                var model = new UnivariateViewModel
                {
                    FileName = fileName,
                    AvailableColumns = columns
                };

                return View("SelectColumn", model);
            }
            catch (Exception ex)
            {
                return View("Index", new UnivariateViewModel { Message = $"Unexpected error: {ex.Message}" });
            }
        }
        [HttpPost]
        public IActionResult Analyze(string fileName, string columnName)
        {
            string html = $"<div class='bg-error'>Analysis failed</div>";
            try
            {
                // htmldecode the filename and column name
                fileName = System.Net.WebUtility.HtmlDecode(fileName);
                columnName = System.Net.WebUtility.HtmlDecode(columnName);



                var outputFolder = _configuration["CsvOutputFolder"];
                var filePath = Path.Combine(outputFolder, fileName);

                if (!System.IO.File.Exists(filePath) || string.IsNullOrEmpty(columnName))
                {
                    html = $"<div class='bg-error'>{fileName} not found in output folder</div>";
                    return Content(html, "text/html");
                }

                // Load CSV and perform univariate analysis
                var dataTable = LoadCsv(filePath);

                // Check if the column exists in the data table
                if (!dataTable.Columns.Contains(columnName))
                {
                    html = $"<div class='bg-error'>Column '{columnName}' does not exist in the data table</div>";
                    return Content(html, "text/html");
                }
                var columnData = dataTable.AsEnumerable().Select(row => row[columnName].ToString()).ToList();
                var isNumeric = columnData.All(value => double.TryParse(value, out _));
                var plt = new ScottPlot.Plot();
                if (isNumeric)
                {
                    var data = dataTable.AsEnumerable()
                                        .Select(row => Convert.ToDouble(row[columnName]))
                                        .ToArray();

                    // Create a histogram using ScottPlot 5.0
                    plt.Add.Bars(data);
                    plt.Title($"Univariate Analysis of {columnName}");
                    plt.XLabel(columnName);
                    plt.YLabel("Frequency");
                }
                else
                {
                    // Handle categorical data
                    var valueCounts = columnData.GroupBy(x => x)
                                                .ToDictionary(g => g.Key, g => g.Count());

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
                // Generate SVG and return as HTML
                string svg = plt.GetSvgXml(600, 400);
                html = $"<div>{svg}</div>";
                return Content(html, "text/html");

            }
            catch (Exception ex)
            {
                html = $"<div class='bg-error'>{ex.Message}</div>";
                return Content(html, "text/html");
            }
        }


        private DataTable LoadCsv(string filePath)
        {
            var dt = new DataTable();
            using (var reader = new StreamReader(filePath))
            {
                var headers = reader.ReadLine()?.Split(',');
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        dt.Columns.Add(header);
                    }

                    while (!reader.EndOfStream)
                    {
                        var rows = reader.ReadLine()?.Split(',');
                        if (rows != null && rows.Length == dt.Columns.Count)
                        {
                            dt.Rows.Add(rows);
                        }
                    }
                }
            }
            return dt;
        }

    }
}
