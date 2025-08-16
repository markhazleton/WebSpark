# CSV AI Processing Service - Enhanced Functionality

## Overview

The `OpenAIFileAnalysisService` has been enhanced to support managing a list of uploaded CSV files that can be reused for multiple analyses. This allows users to:

1. Upload CSV files and keep them for future analysis
2. Analyze multiple uploaded files together
3. Manage the list of uploaded files
4. Perform comparative analysis across multiple datasets

## New Features

### 1. File Management

- **Upload and Register**: Upload CSV files and keep them in a managed list
- **List Uploaded Files**: View all currently uploaded files with metadata
- **Remove Files**: Remove individual files from the list and optionally from OpenAI
- **Clear All Files**: Remove all uploaded files at once

### 2. Multi-File Analysis

- **Analyze Selected Files**: Choose specific files from the uploaded list for analysis
- **Analyze All Files**: Analyze all uploaded files together
- **Custom Prompts**: Use custom analysis prompts for specific insights

### 3. Enhanced Single File Analysis

- **Keep or Delete Option**: Choose whether to keep uploaded files for future use
- **Improved File Handling**: Better temp file management and error handling

## New Methods in OpenAIFileAnalysisService

### File Upload and Management

```csharp
// Upload and register a file for future use
Task<UploadedCsvFile> UploadAndRegisterCsvFileAsync(string filePath)

// Get list of all uploaded files
IReadOnlyList<UploadedCsvFile> GetUploadedFiles()

// Remove a specific file
Task<bool> RemoveUploadedFileAsync(string fileId, bool deleteFromOpenAI = true)

// Clear all uploaded files
Task ClearAllUploadedFilesAsync(bool deleteFromOpenAI = true)
```

### Multi-File Analysis

```csharp
// Analyze specific files by their IDs
Task<string> AnalyzeUploadedCsvFilesAsync(List<string> fileIds, string userPrompt)

// Analyze files by their names
Task<string> AnalyzeUploadedCsvFilesByNameAsync(List<string> fileNames, string userPrompt)

// Analyze all uploaded files
Task<string> AnalyzeAllUploadedCsvFilesAsync(string userPrompt)
```

### Enhanced Single File Analysis

```csharp
// Enhanced method with option to keep the file
Task<string> AnalyzeCsvFileAsync(string filePath, string userPrompt, bool keepFileUploaded = false)
```

## New Controller Actions

### File Upload

- **Upload**: Upload a file with option to keep for future use
- **AnalyzeUploadedFiles**: Analyze selected files from the uploaded list
- **AnalyzeAllFiles**: Analyze all uploaded files
- **RemoveFile**: Remove a specific file from the list
- **ClearAllFiles**: Clear all uploaded files

## Enhanced User Interface

The view now includes:

1. **Upload Section**: File upload with checkbox to keep files for future use
2. **File Management Table**: Shows all uploaded files with:
   - File selection checkboxes
   - File metadata (name, upload date, size)
   - Individual remove actions
3. **Analysis Controls**:
   - Custom prompt input
   - Analyze selected files button
   - Analyze all files button
   - Clear all files button

## Usage Examples

### Upload and Keep File for Future Use

```csharp
var uploadedFile = await _aiService.UploadAndRegisterCsvFileAsync("path/to/file.csv");
```

### Analyze Multiple Files

```csharp
var fileIds = new List<string> { "file1_id", "file2_id" };
var analysis = await _aiService.AnalyzeUploadedCsvFilesAsync(fileIds, 
    "Compare these datasets and identify trends and differences.");
```

### Analyze All Uploaded Files

```csharp
var analysis = await _aiService.AnalyzeAllUploadedCsvFilesAsync(
    "Provide a comprehensive analysis of all uploaded datasets.");
```

## Benefits

1. **Efficiency**: Upload files once, analyze multiple times
2. **Comparative Analysis**: Analyze multiple datasets together for insights
3. **File Management**: Easy management of uploaded files
4. **Flexibility**: Choose between immediate analysis or keeping files for later
5. **Cost Optimization**: Reuse uploaded files instead of re-uploading

## Technical Notes

- Files are stored in memory during the application lifetime
- File cleanup is handled automatically when removing files
- The service maintains metadata about uploaded files (name, size, upload date)
- Error handling for missing or invalid files
- Support for custom analysis prompts
