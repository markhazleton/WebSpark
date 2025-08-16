# Chart Saving Issues - Debugging Guide

## Common Issues and Solutions

### 1. Model Validation Failures

**Problem**: The ChartConfiguration model requires:

- Name (required, max 100 characters)
- CsvFile (required, max 255 characters)  
- ChartType (required, max 50 characters)
- At least one Series with:
  - Name (required, max 100 characters)
  - DataColumn (required, max 100 characters)
  - AggregationFunction (required, max 50 characters)

**Solution**: Check that all required fields are populated in the form.

### 2. Series Data Not Being Bound

**Problem**: The dynamic series inputs may not be properly bound to the model.

**Check**: Look at the Configure.cshtml around line 130-170 for series binding.

### 3. CsvFile Field Binding Issue

**Problem**: Error "Configuration.CsvFile: The CsvFile field is required" even when CSV file is selected.

**Root Cause**: The DataSource field is separate from Configuration.CsvFile, and the binding between them was missing.

**Solution**:

- Added hidden field: `<input type="hidden" asp-for="Configuration.CsvFile" value="@Model.DataSource" />`
- Enhanced controller to ensure: `model.Configuration.CsvFile = model.DataSource`

### 4. Model Type Mismatch Errors

**Problem**: Error "The model item passed into the ViewDataDictionary is of type 'ChartIndexViewModel', but this ViewDataDictionary instance requires a model item of type 'ChartConfigurationViewModel'."

**Root Cause**: The error handling was returning the wrong view model type for the Configure view.

**Solution**: Updated exception handling in ChartController to return the correct ChartConfigurationViewModel for the Configure view instead of using HandleChartException which returns ChartIndexViewModel.

### 5. Numeric Columns Read as Strings

**Problem**: Numeric columns from CSV files are being treated as strings, preventing proper chart calculations and aggregations.

**Root Cause**: The GetColumnsAsync method in ChartDataService was hardcoding all columns as "string" type without proper type detection.

**Solution**: Enhanced ChartDataService.GetColumnsAsync to:

- Use DataFrame type detection from CsvFileService
- Properly identify Integer, Decimal, DateTime, Boolean, and String types
- Perform content analysis for string columns that might contain numeric data
- Set IsNumeric flag correctly for proper chart processing

**Verification**: Check the application logs for messages like "Detected 5 columns with types: day(DateTime), expected(String), count(Decimal)" to confirm proper type detection.

### 6. Chart Type Validation Errors

**Problem**: Error "Value cannot be null. (Parameter 'key')" when validating chart configuration due to null ChartType.

**Root Cause**: ChartTypes.GetInfo() method was being called with null values, and the validation logic didn't check for null ChartType before processing.

**Solution**:

- Added null check in ChartTypes.GetInfo() method
- Enhanced ChartController to validate ChartType is not null before processing
- Improved ChartValidationService to check for empty ChartType before validation

### 7. Chart Not Found After Save

**Problem**: Error "Chart configuration with ID X not found" after successfully saving a chart.

**Root Cause**: Redirect logic was trying to load the saved chart by ID immediately after save, but the in-memory repository might not have been fully updated.

**Solution**: Changed redirect logic after save to redirect to Index with dataSource parameter instead of trying to load the specific saved chart, allowing users to see their saved charts in the list.

### 8. Chart Saved Successfully But Not Visible on Index

**Problem**: Chart is saved successfully with a success message and ID, but doesn't appear in the chart list on the Index page.

**Root Cause**: Charts were being filtered by data source on the Index page, but if the chart was saved without a proper CsvFile (data source) value, it wouldn't match the filter.

**Solution**:

- Enhanced logging to show data source binding issues during save
- Modified Index page to show "All Charts" option that displays all saved charts regardless of data source
- Fixed repository filtering logic to handle null data source parameter correctly
- Added better error logging when DataSource is null during save

**Verification**:

- Check logs for "Chart Index: Found X saved configurations for data source 'Y'" messages
- Look for "DataSource is null or empty" warnings during save
- Use the "All Charts" option in the Index page to see charts without data sources

### 9. Anti-Forgery Token Issues

**Problem**: The form includes `@Html.AntiForgeryToken()` but the POST method expects `[ValidateAntiForgeryToken]`.

**Solution**: Ensure the token is included in the form submission.

### 10. Chart Service Dependencies

**Problem**: Chart services may not be properly initialized.

**Check**: The ChartController constructor requires all these services:

- IChartService
- IDataService  
- IChartRenderingService
- IChartValidationService

### 11. Data Source Validation

**Problem**: The chart validation checks if the CSV file exists and columns are valid.

**Solution**: Ensure the data source file exists in the wwwroot/files directory.

## Quick Debug Steps

1. **Check Browser Console**: Look for JavaScript errors during form submission
2. **Check Application Logs**: Look for validation errors in the terminal output
3. **Verify Form Data**: Use browser dev tools to inspect form data being submitted
4. **Test with Minimal Configuration**: Try saving with just name, type, and one series

## Enhanced Logging & Error Messages

### Logging Improvements Added

1. **Chart Configuration Save**: Detailed logging of save attempts with configuration details
2. **Model State Validation**: Specific field-level validation errors logged and shown to user
3. **Service Initialization**: Logging when chart services are not properly initialized
4. **Data Source Validation**: Detailed logging of CSV file and column validation
5. **Series Validation**: Individual series validation with specific error messages

### User-Friendly Error Messages

- **Missing Chart Name**: "Chart name is required."
- **No Series**: "At least one data series is required. Please add a series with a name and data column."
- **Invalid Series**: "Series 'X' requires a data column selection."
- **Data Source Issues**: "Data source 'filename.csv' is not valid or accessible"
- **Field Validation**: "Please correct the validation errors: [specific field errors]"

### Debug Output Locations

1. **Terminal/Console**: Detailed technical logs for developers
2. **User Interface**: Clear, actionable error messages for end users
3. **Browser Console**: JavaScript validation and preview errors

## Test Configuration

Try saving a chart with these minimal values:

- Name: "Test Chart"
- Chart Type: "Column"
- CSV File: [select existing file]
- Add one series with:
  - Name: "Series 1"
  - DataColumn: [select available column]
  - AggregationFunction: "Sum"

## Advanced Debugging

### Enable Detailed Logging

Add to appsettings.Development.json:

```json
{
  "Logging": {
    "LogLevel": {
      "DataSpark.Web.Controllers.ChartController": "Debug",
      "DataSpark.Web.Services.Chart": "Debug"
    }
  }
}
```

### Common Error Patterns

1. **Empty ModelState**: Usually indicates form data not being bound correctly
2. **Null Configuration**: Form submission without proper model binding
3. **Service Not Initialized**: Missing dependency injection registration
4. **Data Source Not Found**: CSV file doesn't exist in wwwroot/files
5. **Column Mismatch**: Selected column doesn't exist in the CSV file
