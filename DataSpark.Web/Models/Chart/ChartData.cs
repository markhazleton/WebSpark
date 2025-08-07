namespace DataSpark.Web.Models.Chart;

/// <summary>
/// Represents processed chart data ready for rendering
/// </summary>
public class ProcessedChartData
{
    public List<ChartDataPoint> DataPoints { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<string> SeriesNames { get; set; } = new();
    public Dictionary<string, DataType> ColumnTypes { get; set; } = new();
    public int TotalRows { get; set; }
    public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;
    public string ProcessingNotes { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets data points for a specific series
    /// </summary>
    public List<ChartDataPoint> GetSeriesData(string seriesName)
    {
        return DataPoints.Where(dp => dp.SeriesName == seriesName).ToList();
    }

    /// <summary>
    /// Gets all unique categories
    /// </summary>
    public List<string> GetUniqueCategories()
    {
        return DataPoints.Select(dp => dp.Category).Distinct().OrderBy(c => c).ToList();
    }

    /// <summary>
    /// Gets summary statistics for the processed data
    /// </summary>
    public DataSummary GetSummary()
    {
        return new DataSummary
        {
            TotalDataPoints = DataPoints.Count,
            SeriesCount = SeriesNames.Count,
            CategoryCount = Categories.Count,
            MinValue = DataPoints.Any() ? DataPoints.Min(dp => dp.Value) : 0,
            MaxValue = DataPoints.Any() ? DataPoints.Max(dp => dp.Value) : 0,
            AverageValue = DataPoints.Any() ? DataPoints.Average(dp => dp.Value) : 0
        };
    }
}

/// <summary>
/// Represents a single data point in a chart
/// </summary>
public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public string SeriesName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public string? Color { get; set; }
    public bool IsHighlighted { get; set; }
    public string? Tooltip { get; set; }

    /// <summary>
    /// Gets a property value by key
    /// </summary>
    public T? GetProperty<T>(string key)
    {
        if (Properties.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return default;
    }

    /// <summary>
    /// Sets a property value
    /// </summary>
    public void SetProperty(string key, object value)
    {
        Properties[key] = value;
    }
}

/// <summary>
/// Summary statistics for processed chart data
/// </summary>
public class DataSummary
{
    public int TotalDataPoints { get; set; }
    public int SeriesCount { get; set; }
    public int CategoryCount { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double AverageValue { get; set; }
}

/// <summary>
/// Represents the result of chart rendering
/// </summary>
public class ChartRenderResult
{
    public bool Success { get; set; }
    public string? ChartHtml { get; set; }
    public string? ChartJson { get; set; }
    public string? ChartScript { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Creates a successful render result
    /// </summary>
    public static ChartRenderResult CreateSuccess(string html, string json, string script)
    {
        return new ChartRenderResult
        {
            Success = true,
            ChartHtml = html,
            ChartJson = json,
            ChartScript = script
        };
    }

    /// <summary>
    /// Creates a failed render result
    /// </summary>
    public static ChartRenderResult CreateFailure(params string[] errors)
    {
        return new ChartRenderResult
        {
            Success = false,
            Errors = errors.ToList()
        };
    }
}

/// <summary>
/// Represents available chart types and their properties
/// </summary>
public static class ChartTypes
{
    public static readonly Dictionary<string, ChartTypeInfo> All = new()
    {
        { "Column", new ChartTypeInfo("Column", "Column Chart", "Vertical bars for comparing values", true, false) },
        { "StackedColumn", new ChartTypeInfo("StackedColumn", "Stacked Column", "Stacked vertical bars", true, false) },
        { "StackedColumn100", new ChartTypeInfo("StackedColumn100", "100% Stacked Column", "Stacked bars showing percentages", true, false) },
        { "Bar", new ChartTypeInfo("Bar", "Bar Chart", "Horizontal bars for comparing values", true, false) },
        { "StackedBar", new ChartTypeInfo("StackedBar", "Stacked Bar", "Stacked horizontal bars", true, false) },
        { "StackedBar100", new ChartTypeInfo("StackedBar100", "100% Stacked Bar", "Stacked horizontal bars showing percentages", true, false) },
        { "Line", new ChartTypeInfo("Line", "Line Chart", "Connected data points over time", false, true) },
        { "Spline", new ChartTypeInfo("Spline", "Spline Chart", "Smooth curved line chart", false, true) },
        { "StepLine", new ChartTypeInfo("StepLine", "Step Line", "Step-wise connected points", false, true) },
        { "Area", new ChartTypeInfo("Area", "Area Chart", "Filled area under line", false, true) },
        { "SplineArea", new ChartTypeInfo("SplineArea", "Spline Area", "Smooth curved area chart", false, true) },
        { "StackedArea", new ChartTypeInfo("StackedArea", "Stacked Area", "Stacked filled areas", false, true) },
        { "StackedArea100", new ChartTypeInfo("StackedArea100", "100% Stacked Area", "Stacked areas showing percentages", false, true) },
        { "Pie", new ChartTypeInfo("Pie", "Pie Chart", "Circular chart showing proportions", false, false) },
        { "Doughnut", new ChartTypeInfo("Doughnut", "Doughnut Chart", "Pie chart with hollow center", false, false) },
        { "Point", new ChartTypeInfo("Point", "Point Chart", "Individual data points", false, false) },
        { "Bubble", new ChartTypeInfo("Bubble", "Bubble Chart", "Points with varying sizes", false, false) },
        { "Scatter", new ChartTypeInfo("Scatter", "Scatter Plot", "X-Y coordinate plotting", false, false) },
        { "Radar", new ChartTypeInfo("Radar", "Radar Chart", "Multi-variable circular chart", false, false) },
        { "Polar", new ChartTypeInfo("Polar", "Polar Chart", "Circular coordinate system", false, false) }
    };

    public static ChartTypeInfo? GetInfo(string chartType)
    {
        if (string.IsNullOrWhiteSpace(chartType))
            return null;

        return All.TryGetValue(chartType, out var info) ? info : null;
    }

    public static List<string> GetNames() => All.Keys.ToList();
}

/// <summary>
/// Information about a specific chart type
/// </summary>
public class ChartTypeInfo
{
    public string Key { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool SupportsStacking { get; set; }
    public bool RequiresContinuousXAxis { get; set; }

    public ChartTypeInfo(string key, string name, string description, bool supportsStacking, bool requiresContinuousXAxis)
    {
        Key = key;
        Name = name;
        Description = description;
        SupportsStacking = supportsStacking;
        RequiresContinuousXAxis = requiresContinuousXAxis;
    }
}

/// <summary>
/// Available color palettes for charts
/// </summary>
public static class ColorPalettes
{
    public static readonly Dictionary<string, string[]> All = new()
    {
        { "BrightPastel", new[] { "#418CF0", "#FCB441", "#E0400A", "#056492", "#BFBFBF", "#1A3B69", "#FFE382", "#129CDD", "#CA6B4B", "#005CDB" } },
        { "Grayscale", new[] { "#4C4C4C", "#999999", "#333333", "#808080", "#B3B3B3", "#1A1A1A", "#E6E6E6", "#666666", "#CCCCCC", "#000000" } },
        { "Excel", new[] { "#5B9BD5", "#ED7D31", "#A5A5A5", "#FFC000", "#4472C4", "#70AD47", "#FF6600", "#9966CC", "#00B050", "#990000" } },
        { "Fire", new[] { "#FFD700", "#FF6347", "#FF4500", "#DC143C", "#B22222", "#8B0000", "#FFA500", "#FF8C00", "#FF1493", "#800080" } },
        { "Light", new[] { "#E6F3FF", "#FFE6CC", "#FFCCCC", "#E6FFCC", "#CCE6FF", "#F0E6FF", "#FFFFCC", "#FFCCFF", "#CCF0FF", "#F5F5DC" } },
        { "Pastel", new[] { "#FFB3BA", "#FFDFBA", "#FFFFBA", "#BAFFC9", "#BAE1FF", "#C9BAFF", "#FFBAF0", "#F0FFBA", "#BAFFFF", "#FFBADF" } },
        { "SeaGreen", new[] { "#2E8B57", "#3CB371", "#66CDAA", "#98FB98", "#90EE90", "#00FA9A", "#00FF7F", "#7FFF00", "#ADFF2F", "#32CD32" } },
        { "Berry", new[] { "#8B008B", "#9370DB", "#9932CC", "#BA55D3", "#DA70D6", "#EE82EE", "#DDA0DD", "#D8BFD8", "#THISTLE", "#E6E6FA" } }
    };

    public static string[] GetColors(string palette)
    {
        return All.TryGetValue(palette, out var colors) ? colors : All["BrightPastel"];
    }

    public static List<string> GetNames() => All.Keys.ToList();
}

/// <summary>
/// Data type enumeration for columns
/// </summary>
public enum DataType
{
    Unknown,
    String,
    Integer,
    Decimal,
    DateTime,
    Boolean,
    Category
}
