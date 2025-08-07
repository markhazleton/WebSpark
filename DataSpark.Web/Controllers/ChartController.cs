using Microsoft.AspNetCore.Mvc;
using DataSpark.Web.Models.Chart;
using DataSpark.Web.Services.Chart;
using DataSpark.Web.Services;

namespace DataSpark.Web.Controllers;

/// <summary>
/// Main controller for chart configuration and display
/// </summary>
[Route("Chart")]
public class ChartController : BaseController
{
    public ChartController(
        IWebHostEnvironment env,
        ILogger<ChartController> logger,
        CsvFileService csvFileService,
        CsvProcessingService csvProcessingService,
        IChartService chartService,
        IDataService dataService,
        IChartRenderingService renderingService,
        IChartValidationService validationService)
        : base(env, logger, csvFileService, csvProcessingService, chartService, dataService, renderingService, validationService)
    {
    }

    /// <summary>
    /// Main chart index page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? dataSource = null)
    {
        try
        {
            if (_dataService == null || _chartService == null)
                throw new InvalidOperationException("Chart services not properly initialized");

            var availableDataSources = await _dataService.GetAvailableDataSourcesAsync();
            var activeDataSource = dataSource ?? availableDataSources.FirstOrDefault();

            var viewModel = new ChartIndexViewModel
            {
                AvailableDataSources = availableDataSources,
                ActiveDataSource = activeDataSource,
                SavedConfigurations = activeDataSource != null
                    ? await _chartService.GetConfigurationsAsync(activeDataSource)
                    : await _chartService.GetConfigurationsAsync() // Get all charts when no data source is specified
            };

            // Log how many charts were found
            _logger.LogInformation("Chart Index: Found {ChartCount} saved configurations for data source '{DataSource}'",
                viewModel.SavedConfigurations.Count, activeDataSource ?? "ALL");

            // Create a default configuration if no active data source
            if (!string.IsNullOrWhiteSpace(activeDataSource))
            {
                viewModel.CurrentConfiguration = CreateDefaultConfiguration(activeDataSource);
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            return HandleChartException(ex, "Error loading chart index");
        }
    }

    /// <summary>
    /// Chart configuration page
    /// </summary>
    [HttpGet("Configure/{id?}")]
    public async Task<IActionResult> Configure(int? id, string? dataSource = null)
    {
        try
        {
            if (_dataService == null || _chartService == null)
                throw new InvalidOperationException("Chart services not properly initialized");

            ChartConfiguration configuration;

            if (id.HasValue)
            {
                configuration = await _chartService.GetConfigurationAsync(id.Value) ?? throw new InvalidOperationException($"Chart configuration with ID {id} not found");
                if (configuration == null)
                {
                    return NotFound($"Chart configuration with ID {id} not found");
                }
                dataSource = configuration.CsvFile;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dataSource))
                {
                    var availableDataSources = await _dataService.GetAvailableDataSourcesAsync();
                    dataSource = availableDataSources.FirstOrDefault();
                }

                if (string.IsNullOrWhiteSpace(dataSource))
                {
                    return BadRequest("No data source specified and no available data sources found");
                }

                configuration = CreateDefaultConfiguration(dataSource);
            }

            var viewModel = await BuildConfigurationViewModel(configuration, dataSource!);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading chart configuration");
            var errorViewModel = new ChartConfigurationViewModel
            {
                DataSource = dataSource,
                ErrorMessage = $"Error loading chart configuration: {ex.Message}",
                Configuration = CreateDefaultConfiguration(dataSource ?? "")
            };
            return View(errorViewModel);
        }
    }

    /// <summary>
    /// Save chart configuration
    /// </summary>
    [HttpPost("Configure")]
    // [ValidateAntiForgeryToken] // Temporarily disabled for debugging
    public async Task<IActionResult> Configure(ChartConfigurationViewModel model)
    {
        try
        {
            _logger.LogInformation("=== CHART SAVE DEBUG START ===");
            _logger.LogInformation("Raw model received - IsNull: {IsNull}", model == null);

            if (model != null)
            {
                _logger.LogInformation("Model.DataSource: '{DataSource}'", model.DataSource ?? "NULL");
                _logger.LogInformation("Model.Configuration IsNull: {IsNull}", model.Configuration == null);

                if (model.Configuration != null)
                {
                    _logger.LogInformation("Configuration.Name: '{Name}'", model.Configuration.Name ?? "NULL");
                    _logger.LogInformation("Configuration.CsvFile: '{CsvFile}'", model.Configuration.CsvFile ?? "NULL");
                    _logger.LogInformation("Configuration.ChartType: '{ChartType}'", model.Configuration.ChartType ?? "NULL");
                    _logger.LogInformation("Configuration.Series IsNull: {IsNull}, Count: {Count}",
                        model.Configuration.Series == null, model.Configuration.Series?.Count ?? 0);

                    if (model.Configuration.Series != null)
                    {
                        for (int i = 0; i < model.Configuration.Series.Count; i++)
                        {
                            var series = model.Configuration.Series[i];
                            _logger.LogInformation("Series[{Index}]: Name='{Name}', DataColumn='{DataColumn}', AggregationFunction='{Aggregation}'",
                                i, series.Name ?? "NULL", series.DataColumn ?? "NULL", series.AggregationFunction ?? "NULL");
                        }
                    }
                }
            }
            _logger.LogInformation("=== CHART SAVE DEBUG END ===");

            if (_dataService == null || _chartService == null || _validationService == null)
                throw new InvalidOperationException("Chart services not properly initialized");

            // Enhanced debugging - log what we received
            _logger.LogInformation("Attempting to save chart configuration: Name={Name}, DataSource={DataSource}, CsvFile={CsvFile}, ChartType={ChartType}, SeriesCount={SeriesCount}",
                model?.Configuration?.Name, model?.DataSource, model?.Configuration?.CsvFile, model?.Configuration?.ChartType, model?.Configuration?.Series?.Count ?? 0);

            // Ensure Configuration.CsvFile is properly set from DataSource
            if (model?.Configuration != null && !string.IsNullOrWhiteSpace(model.DataSource))
            {
                model.Configuration.CsvFile = model.DataSource;
                _logger.LogDebug("Set Configuration.CsvFile to DataSource: {DataSource}", model.DataSource);
            }
            else if (model?.Configuration != null)
            {
                _logger.LogWarning("DataSource is null or empty - Configuration.CsvFile will be: {CsvFile}", model.Configuration.CsvFile);
            }

            if (!ModelState.IsValid)
            {
                // Log specific ModelState errors for debugging
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();

                _logger.LogWarning("ModelState validation failed: {Errors}", string.Join("; ", errors));

                // Rebuild view model with validation errors
                var viewModel = await BuildConfigurationViewModel(model?.Configuration ?? CreateDefaultConfiguration(model?.DataSource ?? ""), model?.DataSource ?? "");
                viewModel.ErrorMessage = $"Please correct the validation errors: {string.Join(", ", errors)}";
                return View(viewModel);
            }

            // Additional validation for required fields
            if (string.IsNullOrWhiteSpace(model?.Configuration?.Name))
            {
                var viewModel = await BuildConfigurationViewModel(model?.Configuration ?? CreateDefaultConfiguration(model?.DataSource ?? ""), model?.DataSource ?? "");
                viewModel.ErrorMessage = "Chart name is required.";
                return View(viewModel);
            }

            if (string.IsNullOrWhiteSpace(model?.Configuration?.ChartType))
            {
                var viewModel = await BuildConfigurationViewModel(model?.Configuration ?? CreateDefaultConfiguration(model?.DataSource ?? ""), model?.DataSource ?? "");
                viewModel.ErrorMessage = "Chart type is required.";
                return View(viewModel);
            }

            if (model?.Configuration?.Series == null || model.Configuration.Series.Count == 0)
            {
                var viewModel = await BuildConfigurationViewModel(model?.Configuration ?? CreateDefaultConfiguration(model?.DataSource ?? ""), model?.DataSource ?? "");
                viewModel.ErrorMessage = "At least one data series is required. Please add a series with a name and data column.";
                return View(viewModel);
            }

            // Validate each series
            for (int i = 0; i < model.Configuration.Series.Count; i++)
            {
                var series = model.Configuration.Series[i];
                if (string.IsNullOrWhiteSpace(series.Name))
                {
                    var viewModel = await BuildConfigurationViewModel(model.Configuration, model.DataSource!);
                    viewModel.ErrorMessage = $"Series {i + 1} requires a name.";
                    return View(viewModel);
                }
                if (string.IsNullOrWhiteSpace(series.DataColumn))
                {
                    var viewModel = await BuildConfigurationViewModel(model.Configuration, model.DataSource!);
                    viewModel.ErrorMessage = $"Series '{series.Name}' requires a data column selection.";
                    return View(viewModel);
                }
            }

            // Validate the chart configuration
            var validationResult = await _validationService.ValidateConfigurationAsync(model.Configuration, model.DataSource);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Chart validation failed: {Errors}", string.Join("; ", validationResult.Errors));
                var viewModel = await BuildConfigurationViewModel(model.Configuration, model.DataSource!);
                viewModel.ErrorMessage = $"Configuration validation failed: {string.Join(", ", validationResult.Errors)}";
                return View(viewModel);
            }

            // Save the configuration
            var savedConfiguration = await _chartService.SaveConfigurationAsync(model.Configuration);

            _logger.LogInformation("Successfully saved chart configuration: {Name} with ID {Id}", savedConfiguration.Name, savedConfiguration.Id);
            TempData["SuccessMessage"] = $"Chart configuration '{savedConfiguration.Name}' saved successfully";

            // Check if user wants to redirect to view
            if (Request.Form.ContainsKey("redirectToView"))
            {
                return RedirectToAction(nameof(View), new { id = savedConfiguration.Id });
            }

            // Redirect to index with the data source to show the saved chart
            return RedirectToAction(nameof(Index), new { dataSource = model.DataSource });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving chart configuration: {Name}", model.Configuration?.Name);
            var viewModel = await BuildConfigurationViewModel(model.Configuration ?? CreateDefaultConfiguration(model.DataSource!), model.DataSource!);
            viewModel.ErrorMessage = $"Error saving chart configuration: {ex.Message}";
            return View(viewModel);
        }
    }

    /// <summary>
    /// Display chart
    /// </summary>
    [HttpGet("View/{id}")]
    public async Task<IActionResult> View(int id)
    {
        try
        {
            if (_dataService == null || _chartService == null || _renderingService == null)
                throw new InvalidOperationException("Chart services not properly initialized");

            var configuration = await _chartService.GetConfigurationAsync(id);
            if (configuration == null)
            {
                return NotFound($"Chart configuration with ID {id} not found");
            }

            // Process the data
            var processedData = await _dataService.ProcessDataAsync(configuration.CsvFile, configuration);

            // Render the chart
            var renderResult = await _renderingService.RenderChartAsync(configuration, processedData);

            var viewModel = new ChartDisplayViewModel
            {
                Configuration = configuration,
                ChartJson = renderResult.ChartJson,
                ChartHtml = renderResult.ChartHtml,
                Data = processedData,
                Summary = processedData.GetSummary()
            };

            if (TempData["SuccessMessage"] is string successMessage)
            {
                ViewBag.SuccessMessage = successMessage;
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            return HandleChartException(ex, "Error displaying chart");
        }
    }

    /// <summary>
    /// Chart preview (AJAX)
    /// </summary>
    [HttpPost("Preview")]
    public async Task<IActionResult> Preview([FromBody] ChartPreviewRequest request)
    {
        try
        {
            if (_dataService == null || _validationService == null || _renderingService == null)
                throw new InvalidOperationException("Chart services not properly initialized");

            // Validate the configuration
            var validationResult = await _validationService.ValidateConfigurationAsync(request.Configuration, request.DataSource);
            if (!validationResult.IsValid)
            {
                return Json(new { success = false, errors = validationResult.Errors });
            }

            // Process the data
            var processedData = await _dataService.ProcessDataAsync(request.DataSource!, request.Configuration);

            // Limit data points for preview
            if (processedData.DataPoints.Count > request.MaxDataPoints)
            {
                processedData.DataPoints = processedData.DataPoints.Take(request.MaxDataPoints).ToList();
                processedData.ProcessingNotes += $" (Limited to {request.MaxDataPoints} data points for preview)";
            }

            // Generate chart JSON
            var chartJson = await _renderingService.GenerateChartJsonAsync(request.Configuration, processedData);

            if (request.IncludeData)
            {
                return Json(new
                {
                    success = true,
                    chartJson = chartJson,
                    data = processedData,
                    summary = processedData.GetSummary(),
                    warnings = validationResult.Warnings
                });
            }
            else
            {
                return Json(new
                {
                    success = true,
                    chartJson = chartJson,
                    summary = processedData.GetSummary(),
                    warnings = validationResult.Warnings
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chart preview");
            return Json(new { success = false, errors = new[] { "Error generating preview" } });
        }
    }

    /// <summary>
    /// Delete chart configuration
    /// </summary>
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (_chartService == null)
                throw new InvalidOperationException("Chart services not properly initialized");

            var configuration = await _chartService.GetConfigurationAsync(id);
            if (configuration == null)
            {
                return Json(new { success = false, message = "Chart configuration not found" });
            }

            var result = await _chartService.DeleteConfigurationAsync(id);
            if (result)
            {
                return Json(new { success = true, message = $"Chart '{configuration.Name}' deleted successfully" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete chart configuration" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chart configuration {Id}", id);
            return Json(new { success = false, message = "Error deleting chart configuration" });
        }
    }

    /// <summary>
    /// Duplicate chart configuration
    /// </summary>
    [HttpPost("Duplicate/{id}")]
    public async Task<IActionResult> Duplicate(int id, [FromForm] string newName)
    {
        try
        {
            if (_chartService == null)
                throw new InvalidOperationException("Chart services not properly initialized");

            if (string.IsNullOrWhiteSpace(newName))
            {
                return Json(new { success = false, message = "New name is required" });
            }

            var duplicatedConfig = await _chartService.DuplicateConfigurationAsync(id, newName);

            return Json(new
            {
                success = true,
                message = $"Chart duplicated as '{newName}'",
                newId = duplicatedConfig.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating chart configuration {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Embed chart (for iframe embedding)
    /// </summary>
    [HttpGet("Embed/{id}")]
    public async Task<IActionResult> Embed(int id)
    {
        try
        {
            if (_dataService == null || _chartService == null || _renderingService == null)
                throw new InvalidOperationException("Chart services not properly initialized");

            var configuration = await _chartService.GetConfigurationAsync(id);
            if (configuration == null)
            {
                return NotFound();
            }

            var processedData = await _dataService.ProcessDataAsync(configuration.CsvFile, configuration);
            var renderResult = await _renderingService.RenderChartAsync(configuration, processedData);

            var viewModel = new ChartDisplayViewModel
            {
                Configuration = configuration,
                Data = processedData,
                ChartJson = renderResult.ChartJson,
                ChartHtml = renderResult.ChartHtml,
                IsEditable = false
            };

            return View("Embed", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading embedded chart {Id}", id);
            return StatusCode(500, "Error loading chart");
        }
    }
}
