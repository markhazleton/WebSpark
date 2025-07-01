using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DataSpark.Web.Controllers;

public class CsvAIProcessingController : Controller
{
    private readonly OpenAIFileAnalysisService _aiService;
    private readonly CsvFileService _csvFileService;

    public CsvAIProcessingController(OpenAIFileAnalysisService aiService, CsvFileService csvFileService)
    {
        _aiService = aiService;
        _csvFileService = csvFileService;
    }

    public IActionResult Index(string? fileName)
    {
        var availableFiles = _csvFileService.GetCsvFileNames();

        ViewBag.AvailableFiles = availableFiles;
        ViewBag.SelectedFile = fileName;
        ViewBag.UploadedFiles = _aiService.GetUploadedFiles();

        if (string.IsNullOrEmpty(fileName) && availableFiles.Any())
        {
            ViewBag.SelectedFile = availableFiles.First();
        }

        if (availableFiles.Count == 0)
        {
            ViewBag.ErrorMessage = "No CSV files found. Please upload files to the system first.";
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeFile(string fileName, string customPrompt = "")
    {
        try
        {
            var availableFiles = _csvFileService.GetCsvFileNames();
            ViewBag.AvailableFiles = availableFiles;
            ViewBag.SelectedFile = fileName;
            ViewBag.UploadedFiles = _aiService.GetUploadedFiles();

            if (string.IsNullOrEmpty(fileName))
            {
                ViewBag.Analysis = "Please select a file to analyze.";
                return View("Index");
            }

            if (!availableFiles.Contains(fileName))
            {
                ViewBag.Analysis = $"File '{fileName}' not found in available files.";
                return View("Index");
            }

            // Get the full path to the CSV file
            var filePath = _csvFileService.GetCsvFilePath(fileName);

            var prompt = string.IsNullOrWhiteSpace(customPrompt)
                ? "Analyze and summarize this CSV file. Please load the CSV file, examine its structure, and provide detailed insights including data types, summary statistics, and any patterns you observe."
                : customPrompt;

            // Use the enhanced analysis method with verification
            string analysis = await _aiService.AnalyzeCsvFileWithVerificationAsync(filePath, prompt, false);

            ViewBag.Analysis = analysis;
            return View("Index");
        }
        catch (Exception ex)
        {
            var availableFiles = _csvFileService.GetCsvFileNames();
            ViewBag.AvailableFiles = availableFiles;
            ViewBag.SelectedFile = fileName;
            ViewBag.UploadedFiles = _aiService.GetUploadedFiles();
            ViewBag.Analysis = $"Error analyzing file: {ex.Message}";
            return View("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadAndRegisterFile(string fileName)
    {
        try
        {
            var availableFiles = _csvFileService.GetCsvFileNames();
            ViewBag.AvailableFiles = availableFiles;
            ViewBag.SelectedFile = fileName;

            if (string.IsNullOrEmpty(fileName) || !availableFiles.Contains(fileName))
            {
                ViewBag.Analysis = "Please select a valid file to upload and register.";
                ViewBag.UploadedFiles = _aiService.GetUploadedFiles();
                return View("Index");
            }

            // Get the full path to the CSV file
            var filePath = _csvFileService.GetCsvFilePath(fileName);

            // Upload and register the file for future use
            var uploadedFile = await _aiService.UploadAndRegisterCsvFileAsync(filePath);
            ViewBag.Analysis = $"File '{uploadedFile.FileName}' uploaded and registered successfully for future analysis. File ID: {uploadedFile.FileId}";
            ViewBag.UploadedFiles = _aiService.GetUploadedFiles();
            return View("Index");
        }
        catch (Exception ex)
        {
            var availableFiles = _csvFileService.GetCsvFileNames();
            ViewBag.AvailableFiles = availableFiles;
            ViewBag.SelectedFile = fileName;
            ViewBag.UploadedFiles = _aiService.GetUploadedFiles();
            ViewBag.Analysis = $"Error uploading file: {ex.Message}";
            return View("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeUploadedFiles(List<string> selectedFileIds, string customPrompt = "")
    {
        try
        {
            var availableFiles = _csvFileService.GetCsvFileNames();
            ViewBag.AvailableFiles = availableFiles;
            ViewBag.UploadedFiles = _aiService.GetUploadedFiles();

            if (selectedFileIds == null || !selectedFileIds.Any())
            {
                ViewBag.Analysis = "No files selected for analysis.";
                return View("Index");
            }

            var prompt = string.IsNullOrWhiteSpace(customPrompt)
                ? "Analyze and summarize these CSV files. Compare their contents and provide insights."
                : customPrompt;

            string analysis = await _aiService.AnalyzeUploadedCsvFilesAsync(selectedFileIds, prompt);
            ViewBag.Analysis = analysis;
            return View("Index");
        }
        catch (Exception ex)
        {
            var availableFiles = _csvFileService.GetCsvFileNames();
            ViewBag.AvailableFiles = availableFiles;
            ViewBag.UploadedFiles = _aiService.GetUploadedFiles();
            ViewBag.Analysis = $"Error: {ex.Message}";
            return View("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeAllFiles(string customPrompt = "")
    {
        try
        {
            var availableFiles = _csvFileService.GetCsvFileNames();
            ViewBag.AvailableFiles = availableFiles;
            ViewBag.UploadedFiles = _aiService.GetUploadedFiles();

            var prompt = string.IsNullOrWhiteSpace(customPrompt)
                ? "Analyze and summarize all uploaded CSV files. Compare their contents and provide comprehensive insights."
                : customPrompt;

            string analysis = await _aiService.AnalyzeAllUploadedCsvFilesAsync(prompt);
            ViewBag.Analysis = analysis;
            return View("Index");
        }
        catch (Exception ex)
        {
            var availableFiles = _csvFileService.GetCsvFileNames();
            ViewBag.AvailableFiles = availableFiles;
            ViewBag.UploadedFiles = _aiService.GetUploadedFiles();
            ViewBag.Analysis = $"Error: {ex.Message}";
            return View("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFile(string fileId)
    {
        try
        {
            bool removed = await _aiService.RemoveUploadedFileAsync(fileId, true);
            ViewBag.Analysis = removed ? "File removed successfully." : "File not found.";
        }
        catch (Exception ex)
        {
            ViewBag.Analysis = $"Error removing file: {ex.Message}";
        }

        var availableFiles = _csvFileService.GetCsvFileNames();
        ViewBag.AvailableFiles = availableFiles;
        ViewBag.UploadedFiles = _aiService.GetUploadedFiles();
        return View("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ClearAllFiles()
    {
        try
        {
            await _aiService.ClearAllUploadedFilesAsync(true);
            ViewBag.Analysis = "All files cleared successfully.";
        }
        catch (Exception ex)
        {
            ViewBag.Analysis = $"Error clearing files: {ex.Message}";
        }

        var availableFiles = _csvFileService.GetCsvFileNames();
        ViewBag.AvailableFiles = availableFiles;
        ViewBag.UploadedFiles = _aiService.GetUploadedFiles();
        return View("Index");
    }

    [HttpPost]
    public async Task<IActionResult> RunDiagnostics()
    {
        try
        {
            string diagnostics = await _aiService.DiagnoseConfigurationAsync();
            ViewBag.Analysis = diagnostics;
        }
        catch (Exception ex)
        {
            ViewBag.Analysis = $"Diagnostics failed: {ex.Message}";
        }

        var availableFiles = _csvFileService.GetCsvFileNames();
        ViewBag.AvailableFiles = availableFiles;
        ViewBag.UploadedFiles = _aiService.GetUploadedFiles();
        return View("Index");
    }
}
