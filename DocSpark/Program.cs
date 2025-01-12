using DocSpark;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

string projectPath = "C:\\AzureDevOps\\bswhealth\\HealthSource\\BSW.Core.Api\\BSW.Web.Api";
projectPath = "C:\\GitHub\\MarkHazleton\\PromptSpark.Chat\\PromptSpark.Chat";
projectPath = "C:\\AzureDevOps\\bswhealth\\HealthSource\\BSW.EHR\\BSW.EHR.V2";

// Configure services and logging
var serviceProvider = new ServiceCollection()
    .AddLogging(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
    })
    .AddSingleton<FileService>()
    .AddSingleton<SyntaxAnalysisService>()
    .AddSingleton<MarkdownGenerator>()
    .BuildServiceProvider();

var logger = serviceProvider.GetService<ILogger<Program>>();
var fileService = serviceProvider.GetService<FileService>();
var syntaxAnalysisService = serviceProvider.GetService<SyntaxAnalysisService>();
var markdownGenerator = serviceProvider.GetService<MarkdownGenerator>();


if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
{
    logger?.LogError("Invalid path provided. Please provide a valid directory.");
    return;
}

string? jsonOutputPath = null;
if (string.IsNullOrWhiteSpace(jsonOutputPath))
{
    jsonOutputPath = Path.Combine(projectPath, "ProjectClasses.json");
}

Console.WriteLine("Enter the output file path for the documentation (default: ProjectDocumentation.md):");
string? outputPath = null;
if (string.IsNullOrWhiteSpace(outputPath))
{
    outputPath = Path.Combine(projectPath, "ProjectDocumentation.md");
}

try
{
    // Retrieve source files
    var sourceFiles = fileService.GetSourceFiles(projectPath);
    if (sourceFiles.Length == 0)
    {
        logger?.LogWarning("No source files found in the provided project path.");
        return;
    }
    var sourceTreeFiles = fileService.GetFileSystemTree(projectPath);

    // Analyze syntax and extract classes
    var classes = syntaxAnalysisService.ExtractClassesFromFileSystemNode(sourceTreeFiles, projectPath);

    // Save class data to JSON file
    await SaveClassesToJsonFile(classes, jsonOutputPath, logger);


    var markdown = markdownGenerator.GenerateMarkdown(classes);
    // Write to the output file
    await fileService.WriteToFileAsync(outputPath, markdown);



    Console.WriteLine($"Documentation generated successfully: {outputPath}");
}
catch (Exception ex)
{
    logger?.LogError(ex, "An error occurred during processing.");
}

static async Task SaveClassesToJsonFile(object classes, string outputPath, ILogger logger)
{
    try
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(classes, options);
        await File.WriteAllTextAsync(outputPath, json);
        logger?.LogInformation("Class data saved successfully to JSON: {OutputPath}", outputPath);
    }
    catch (Exception ex)
    {
        logger?.LogError(ex, "Failed to save class data to JSON.");
    }
}
