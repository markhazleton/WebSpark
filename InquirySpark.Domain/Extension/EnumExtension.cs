using System.Reflection;

namespace InquirySpark.Domain.Extension;

/// <summary>
/// ENUM Extension Methods
/// </summary>
public static class EnumExtension
{
    /// <summary>
    /// Converts to Description String.
    /// </summary>
    /// <typeparam name="TEnum">The type of the t enum.</typeparam>
    /// <param name="enum">The enum.</param>
    /// <returns>System.String.</returns>
    public static string ToDescriptionString<TEnum>(this TEnum @enum)
        where TEnum : struct, Enum
    {
        FieldInfo? info = @enum.GetType()?.GetField(@enum.ToString());
        if (info == null) return string.Empty;
        var attributes = info.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : @enum.ToString();
    }

    /// <summary>
    /// Gets Display CompanyNm of enum
    /// </summary>
    /// <param name="e">enum key name</param>
    /// <returns>enum value</returns>
    public static string GetDisplayName(this Enum e)
    {
        var fieldInfo = e.GetType()?.GetField(e.ToString());

        if (!(fieldInfo?.GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] descriptionAttributes))
            return fieldInfo?.Name ?? string.Empty;

        return descriptionAttributes.Length > 0 ? descriptionAttributes[0].GetName() ?? string.Empty : fieldInfo?.Name ?? string.Empty;
    }

    /// <summary>
    /// Returns whether the given enum value is a defined value for its type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumValue">The enum value.</param>
    /// <returns><c>true</c> if the specified enum value is defined; otherwise, <c>false</c>.</returns>
    public static bool IsDefined<T>(this T enumValue)
        where T : struct, Enum => Enum.IsDefined(typeof(T), enumValue);

    /// <summary>
    /// Generates Dictionary of int,string for an Enum
    /// </summary>
    /// <param name="enumValue">The enum value.</param>
    /// <returns>Dictionary&lt;System.Int32, System.String&gt;.</returns>
    public static Dictionary<int, string> ToDictionary(this Enum enumValue)
    {
        var enumType = enumValue.GetType();
        return Enum.GetValues(enumType).Cast<Enum>().ToDictionary(t => (int)(object)t, t => t.ToString());
    }
}
