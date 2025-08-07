using DataSpark.Web.Models.Chart;
using DataSpark.Web.Services.Chart;
using System.Text.Json;
using System.Text;

namespace DataSpark.Web.Services.Chart;

/// <summary>
/// Implementation of chart rendering service using Chart.js
/// </summary>
public class ChartRenderingService : IChartRenderingService
{
    private readonly ILogger<ChartRenderingService> _logger;

    public ChartRenderingService(ILogger<ChartRenderingService> logger)
    {
        _logger = logger;
    }

    public async Task<ChartRenderResult> RenderChartAsync(ChartConfiguration config, ProcessedChartData data)
    {
        try
        {
            var chartJson = await GenerateChartJsonAsync(config, data);
            var chartHtml = await GenerateChartHtmlAsync(config, data);
            var chartScript = await GenerateChartScriptAsync(config, data);

            return ChartRenderResult.CreateSuccess(chartHtml, chartJson, chartScript);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering chart: {ChartName}", config.Name);
            return ChartRenderResult.CreateFailure($"Error rendering chart: {ex.Message}");
        }
    }

    public async Task<string> GenerateChartJsonAsync(ChartConfiguration config, ProcessedChartData data)
    {
        try
        {
            var chartData = BuildChartData(config, data);
            var chartOptions = BuildChartOptions(config);

            var chartConfig = new
            {
                type = MapChartType(config.ChartType),
                data = chartData,
                options = chartOptions
            };

            return JsonSerializer.Serialize(chartConfig, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chart JSON");
            throw;
        }
    }

    public async Task<string> GenerateChartHtmlAsync(ChartConfiguration config, ProcessedChartData data)
    {
        try
        {
            var chartId = $"chart_{Guid.NewGuid():N}";
            var chartJson = await GenerateChartJsonAsync(config, data);

            var html = new StringBuilder();
            html.AppendLine("<div class=\"chart-container\" style=\"position: relative;\">");
            html.AppendLine($"  <canvas id=\"{chartId}\" width=\"{config.Width}\" height=\"{config.Height}\"></canvas>");
            html.AppendLine("</div>");

            html.AppendLine("<script>");
            html.AppendLine($"document.addEventListener('DOMContentLoaded', function() {{");
            html.AppendLine($"  var ctx = document.getElementById('{chartId}').getContext('2d');");
            html.AppendLine($"  var chartConfig = {chartJson};");
            html.AppendLine($"  var chart = new Chart(ctx, chartConfig);");
            html.AppendLine("});");
            html.AppendLine("</script>");

            return html.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chart HTML");
            throw;
        }
    }

    public async Task<string> GenerateChartScriptAsync(ChartConfiguration config, ProcessedChartData data)
    {
        try
        {
            var chartJson = await GenerateChartJsonAsync(config, data);

            var script = new StringBuilder();
            script.AppendLine("function createChart(canvasId) {");
            script.AppendLine("  var ctx = document.getElementById(canvasId).getContext('2d');");
            script.AppendLine($"  var chartConfig = {chartJson};");
            script.AppendLine("  return new Chart(ctx, chartConfig);");
            script.AppendLine("}");

            return script.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chart script");
            throw;
        }
    }

    public async Task<byte[]> ExportChartAsync(ChartConfiguration config, ProcessedChartData data, string format)
    {
        // This would require a headless browser or server-side rendering solution
        // For now, return empty byte array
        _logger.LogWarning("Chart export not implemented for format: {Format}", format);
        return Array.Empty<byte>();
    }

    public async Task<string> GenerateEmbedCodeAsync(ChartConfiguration config, string baseUrl)
    {
        try
        {
            var embedUrl = $"{baseUrl}/Chart/Embed/{config.Id}";

            var embedCode = new StringBuilder();
            embedCode.AppendLine($"<iframe src=\"{embedUrl}\"");
            embedCode.AppendLine($"        width=\"{config.Width}\"");
            embedCode.AppendLine($"        height=\"{config.Height}\"");
            embedCode.AppendLine("        frameborder=\"0\"");
            embedCode.AppendLine("        scrolling=\"no\"");
            embedCode.AppendLine($"        title=\"{config.Title}\"></iframe>");

            return embedCode.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embed code");
            throw;
        }
    }

    private object BuildChartData(ChartConfiguration config, ProcessedChartData data)
    {
        var chartType = config.ChartType.ToLowerInvariant();

        if (chartType == "pie" || chartType == "doughnut")
        {
            return BuildPieChartData(config, data);
        }
        else
        {
            return BuildStandardChartData(config, data);
        }
    }

    private object BuildPieChartData(ChartConfiguration config, ProcessedChartData data)
    {
        var series = config.Series.FirstOrDefault(s => s.IsVisible);
        if (series == null)
        {
            return new { labels = Array.Empty<string>(), datasets = Array.Empty<object>() };
        }

        var seriesData = data.GetSeriesData(series.Name);
        var labels = seriesData.Select(d => d.Label).ToArray();
        var values = seriesData.Select(d => d.Value).ToArray();
        var colors = GenerateColors(config.ChartPalette, seriesData.Count);

        var dataset = new
        {
            label = series.Name,
            data = values,
            backgroundColor = colors,
            borderColor = colors.Select(c => c).ToArray(),
            borderWidth = 1
        };

        return new
        {
            labels = labels,
            datasets = new[] { dataset }
        };
    }

    private object BuildStandardChartData(ChartConfiguration config, ProcessedChartData data)
    {
        var labels = data.GetUniqueCategories().ToArray();
        var datasets = new List<object>();

        var colors = GenerateColors(config.ChartPalette, config.Series.Count);
        var colorIndex = 0;

        foreach (var series in config.Series.Where(s => s.IsVisible).OrderBy(s => s.DisplayOrder))
        {
            var seriesData = data.GetSeriesData(series.Name);
            var values = labels.Select(label =>
            {
                var dataPoint = seriesData.FirstOrDefault(d => d.Category == label);
                return dataPoint?.Value ?? 0;
            }).ToArray();

            var color = !string.IsNullOrWhiteSpace(series.Color) ? series.Color : colors[colorIndex % colors.Length];
            colorIndex++;

            var dataset = new
            {
                label = series.Name,
                data = values,
                backgroundColor = GetBackgroundColor(color, config.ChartType),
                borderColor = color,
                borderWidth = series.LineWidth,
                fill = GetFillOption(config.ChartType),
                tension = GetTensionOption(config.ChartType)
            };

            datasets.Add(dataset);
        }

        return new
        {
            labels = labels,
            datasets = datasets
        };
    }

    private object BuildChartOptions(ChartConfiguration config)
    {
        var options = new Dictionary<string, object>
        {
            ["responsive"] = true,
            ["maintainAspectRatio"] = false
        };

        // Add title
        if (!string.IsNullOrWhiteSpace(config.Title))
        {
            options["plugins"] = new Dictionary<string, object>
            {
                ["title"] = new Dictionary<string, object>
                {
                    ["display"] = true,
                    ["text"] = config.Title,
                    ["font"] = new { size = 16 }
                },
                ["legend"] = new Dictionary<string, object>
                {
                    ["display"] = config.ShowLegend,
                    ["position"] = config.LegendPosition.ToLowerInvariant()
                }
            };
        }

        // Add scales for non-pie charts
        if (config.ChartType != "Pie" && config.ChartType != "Doughnut")
        {
            var scales = new Dictionary<string, object>();

            // X-axis configuration
            if (config.XAxis != null)
            {
                scales["x"] = BuildAxisOptions(config.XAxis);
            }

            // Y-axis configuration
            if (config.YAxis != null)
            {
                scales["y"] = BuildAxisOptions(config.YAxis);
            }

            if (scales.Any())
            {
                options["scales"] = scales;
            }
        }

        // Add interaction options
        if (config.ShowTooltips)
        {
            if (!options.ContainsKey("plugins"))
                options["plugins"] = new Dictionary<string, object>();

            ((Dictionary<string, object>)options["plugins"])["tooltip"] = new Dictionary<string, object>
            {
                ["enabled"] = true,
                ["mode"] = "index",
                ["intersect"] = false
            };
        }

        // Add animation
        if (config.IsAnimated)
        {
            options["animation"] = new Dictionary<string, object>
            {
                ["duration"] = config.AnimationDuration
            };
        }

        return options;
    }

    private object BuildAxisOptions(ChartAxis axis)
    {
        var axisOptions = new Dictionary<string, object>
        {
            ["display"] = true
        };

        if (!string.IsNullOrWhiteSpace(axis.Title))
        {
            axisOptions["title"] = new Dictionary<string, object>
            {
                ["display"] = true,
                ["text"] = axis.Title
            };
        }

        if (axis.MinValue.HasValue || axis.MaxValue.HasValue)
        {
            axisOptions["min"] = axis.MinValue;
            axisOptions["max"] = axis.MaxValue;
        }

        if (axis.IsLogarithmic)
        {
            axisOptions["type"] = "logarithmic";
        }

        // Grid configuration
        axisOptions["grid"] = new Dictionary<string, object>
        {
            ["display"] = axis.ShowGridLines,
            ["color"] = axis.GridLineColor
        };

        // Tick configuration
        axisOptions["ticks"] = new Dictionary<string, object>
        {
            ["display"] = axis.ShowLabels,
            ["color"] = axis.LabelColor,
            ["font"] = new Dictionary<string, object>
            {
                ["size"] = axis.LabelFontSize
            }
        };

        return axisOptions;
    }

    private string MapChartType(string chartType)
    {
        return chartType.ToLowerInvariant() switch
        {
            "column" => "bar",
            "stackedcolumn" => "bar",
            "stackedcolumn100" => "bar",
            "spline" => "line",
            "splinearea" => "line",
            "stepline" => "line",
            "area" => "line",
            "stackedarea" => "line",
            "stackedarea100" => "line",
            "point" => "scatter",
            "doughnut" => "doughnut",
            _ => chartType.ToLowerInvariant()
        };
    }

    private string[] GenerateColors(string palette, int count)
    {
        var paletteColors = ColorPalettes.GetColors(palette);
        var colors = new string[count];

        for (int i = 0; i < count; i++)
        {
            colors[i] = paletteColors[i % paletteColors.Length];
        }

        return colors;
    }

    private string GetBackgroundColor(string borderColor, string chartType)
    {
        // For area charts, use semi-transparent background
        if (chartType.Contains("Area"))
        {
            return ConvertToRgba(borderColor, 0.2);
        }

        // For bar/column charts, use the same color
        if (chartType.Contains("Bar") || chartType.Contains("Column"))
        {
            return borderColor;
        }

        // For line charts, use transparent
        return "transparent";
    }

    private bool GetFillOption(string chartType)
    {
        return chartType.Contains("Area");
    }

    private double GetTensionOption(string chartType)
    {
        return chartType.Contains("Spline") ? 0.4 : 0;
    }

    private string ConvertToRgba(string hexColor, double alpha)
    {
        if (!hexColor.StartsWith("#") || hexColor.Length != 7)
            return hexColor;

        try
        {
            var r = Convert.ToInt32(hexColor.Substring(1, 2), 16);
            var g = Convert.ToInt32(hexColor.Substring(3, 2), 16);
            var b = Convert.ToInt32(hexColor.Substring(5, 2), 16);

            return $"rgba({r}, {g}, {b}, {alpha})";
        }
        catch
        {
            return hexColor;
        }
    }
}
