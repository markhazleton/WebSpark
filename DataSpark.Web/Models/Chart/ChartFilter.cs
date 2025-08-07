using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DataSpark.Web.Models.Chart;

/// <summary>
/// Represents data filtering configuration for charts
/// </summary>
public class ChartFilter
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("ChartConfiguration")]
    public int ChartConfigurationId { get; set; }

    [Required]
    [StringLength(100)]
    public string Column { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string FilterType { get; set; } = "Include"; // Include, Exclude, Range, Pattern

    // Serialized as JSON arrays
    public string? IncludedValuesJson { get; set; }

    public string? ExcludedValuesJson { get; set; }

    // For range filters
    public string? MinValue { get; set; }

    public string? MaxValue { get; set; }

    // For pattern filters
    [StringLength(200)]
    public string? Pattern { get; set; }

    public bool IsCaseSensitive { get; set; } = false;

    public bool IsRegex { get; set; } = false;

    public bool IsEnabled { get; set; } = true;

    public int DisplayOrder { get; set; }

    // Navigation property
    public ChartConfiguration? ChartConfiguration { get; set; }

    /// <summary>
    /// Gets the included values as a list of strings
    /// </summary>
    [NotMapped]
    public List<string> IncludedValues
    {
        get
        {
            if (string.IsNullOrWhiteSpace(IncludedValuesJson))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(IncludedValuesJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
        set
        {
            IncludedValuesJson = JsonSerializer.Serialize(value);
        }
    }

    /// <summary>
    /// Gets the excluded values as a list of strings
    /// </summary>
    [NotMapped]
    public List<string> ExcludedValues
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ExcludedValuesJson))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(ExcludedValuesJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
        set
        {
            ExcludedValuesJson = JsonSerializer.Serialize(value);
        }
    }

    /// <summary>
    /// Validates the filter configuration
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Column))
            errors.Add("Filter column is required");

        var validFilterTypes = new[] { "Include", "Exclude", "Range", "Pattern" };
        if (!validFilterTypes.Contains(FilterType))
            errors.Add($"Invalid filter type. Valid options: {string.Join(", ", validFilterTypes)}");

        switch (FilterType)
        {
            case "Include":
                if (IncludedValues.Count == 0)
                    errors.Add("Include filter must have at least one value");
                break;

            case "Exclude":
                if (ExcludedValues.Count == 0)
                    errors.Add("Exclude filter must have at least one value");
                break;

            case "Range":
                if (string.IsNullOrWhiteSpace(MinValue) && string.IsNullOrWhiteSpace(MaxValue))
                    errors.Add("Range filter must have at least min or max value");
                break;

            case "Pattern":
                if (string.IsNullOrWhiteSpace(Pattern))
                    errors.Add("Pattern filter must have a pattern");

                if (IsRegex)
                {
                    try
                    {
                        _ = new System.Text.RegularExpressions.Regex(Pattern!);
                    }
                    catch
                    {
                        errors.Add("Invalid regular expression pattern");
                    }
                }
                break;
        }

        return errors;
    }

    /// <summary>
    /// Determines if a value passes this filter
    /// </summary>
    public bool PassesFilter(string? value)
    {
        if (!IsEnabled)
            return true;

        if (value == null)
            return FilterType == "Exclude"; // Null values pass exclude filters, fail include filters

        var compareValue = IsCaseSensitive ? value : value.ToLowerInvariant();

        return FilterType switch
        {
            "Include" => IncludedValues.Any(v =>
                string.Equals(IsCaseSensitive ? v : v.ToLowerInvariant(), compareValue, StringComparison.Ordinal)),

            "Exclude" => !ExcludedValues.Any(v =>
                string.Equals(IsCaseSensitive ? v : v.ToLowerInvariant(), compareValue, StringComparison.Ordinal)),

            "Range" => PassesRangeFilter(value),

            "Pattern" => PassesPatternFilter(value),

            _ => true
        };
    }

    private bool PassesRangeFilter(string value)
    {
        // Try to parse as numeric first
        if (double.TryParse(value, out var numValue))
        {
            double min = 0, max = 0;
            var minParsed = string.IsNullOrWhiteSpace(MinValue) || double.TryParse(MinValue, out min);
            var maxParsed = string.IsNullOrWhiteSpace(MaxValue) || double.TryParse(MaxValue, out max);

            if (minParsed && maxParsed)
            {
                var passesMin = string.IsNullOrWhiteSpace(MinValue) || numValue >= min;
                var passesMax = string.IsNullOrWhiteSpace(MaxValue) || numValue <= max;
                return passesMin && passesMax;
            }
        }

        // Try to parse as date
        if (DateTime.TryParse(value, out var dateValue))
        {
            DateTime minDate = DateTime.MinValue, maxDate = DateTime.MaxValue;
            var minParsed = string.IsNullOrWhiteSpace(MinValue) || DateTime.TryParse(MinValue, out minDate);
            var maxParsed = string.IsNullOrWhiteSpace(MaxValue) || DateTime.TryParse(MaxValue, out maxDate);

            if (minParsed && maxParsed)
            {
                var passesMin = string.IsNullOrWhiteSpace(MinValue) || dateValue >= minDate;
                var passesMax = string.IsNullOrWhiteSpace(MaxValue) || dateValue <= maxDate;
                return passesMin && passesMax;
            }
        }

        // String comparison as fallback
        var comparison = IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var passesMinStr = string.IsNullOrWhiteSpace(MinValue) || string.Compare(value, MinValue, comparison) >= 0;
        var passesMaxStr = string.IsNullOrWhiteSpace(MaxValue) || string.Compare(value, MaxValue, comparison) <= 0;

        return passesMinStr && passesMaxStr;
    }

    private bool PassesPatternFilter(string value)
    {
        if (string.IsNullOrWhiteSpace(Pattern))
            return true;

        var compareValue = IsCaseSensitive ? value : value.ToLowerInvariant();
        var comparePattern = IsCaseSensitive ? Pattern : Pattern.ToLowerInvariant();

        if (IsRegex)
        {
            try
            {
                var options = IsCaseSensitive ? System.Text.RegularExpressions.RegexOptions.None :
                             System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                return System.Text.RegularExpressions.Regex.IsMatch(compareValue, comparePattern, options);
            }
            catch
            {
                return false;
            }
        }

        return compareValue.Contains(comparePattern);
    }
}
