using HttpClientUtility.MemoryCache;
using System.Data;
using WebSpark.Portal.Areas.DataSpark.Models;
using WebSpark.Portal.Areas.DataSpark.Services;

namespace WebSpark.Portal.Areas.DataSpark.Controllers;

public class UnivariateController(
    CsvProcessingService csvProcessingService,
    IMemoryCacheManager memoryCacheManager,
    IConfiguration configuration,
    ILogger<UnivariateController> logger) : DataSparkBaseController<UnivariateController>(memoryCacheManager,configuration, logger)
{
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
            if (string.IsNullOrEmpty(OutputFolder))
            {
                return View("Index", new UnivariateViewModel { Message = "CSV output folder is not configured." });
            }

            // Ensure the directory exists
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            // Set file name and path
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(OutputFolder, fileName);

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



            var outputFolder = configuration["CsvOutputFolder"];
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

            string svg = csvProcessingService.GetScottPlotSvg(columnName, dataTable);
            html = $"<div>{svg}</div>";
            return Content(html, "text/html");

        }
        catch (Exception ex)
        {
            html = $"<div class='bg-error'>{ex.Message}</div>";
            return Content(html, "text/html");
        }
    }

    private static DataTable LoadCsv(string filePath)
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
