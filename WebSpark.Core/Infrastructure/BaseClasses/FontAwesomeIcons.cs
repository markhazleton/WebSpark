
namespace WebSpark.Core.Infrastructure.BaseClasses;

/// <summary>
/// 
/// </summary>
public static class EnumerationExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string Description(this Enum value)
    {
        // get attributes  
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
        {
            return "Description Not Found";
        }
        var attributes = field.GetCustomAttributes(false);

        // Description is in a hidden Attribute class called DisplayAttribute
        // Not to be confused with DisplayNameAttribute
        object? displayAttribute = null;

        if (attributes.Any())
        {
            displayAttribute = attributes.ElementAt(0);
        }

        // return description
        // use reflection to get Description property if present
        if (displayAttribute != null)
        {
            var prop = displayAttribute.GetType().GetProperty("Description", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.GetValue(displayAttribute) is string desc && !string.IsNullOrWhiteSpace(desc))
            {
                return desc;
            }
        }
        return "Description Not Found";
    }
}
/// <summary>
/// FontAwesomeIcons
/// </summary>
public enum FontAwesomeIcons
{
    /// <summary>
    /// 
    /// </summary>
    [Description("fa-home")]
    home,
    /// <summary>
    /// 
    /// </summary>
    [Description("fa-heart")]
    heart,
    /// <summary>
    /// 
    /// </summary>
    [Description("fa-bolt")]
    bolt,
    /// <summary>
    /// 
    /// </summary>
    [Description("fa-envelope")]
    envelope,
    /// <summary>
    /// 
    /// </summary>
    [Description("fa-coffee")]
    coffee,
    /// <summary>
    /// 
    /// </summary>
    [Description("fa-female")]
    female,
    /// <summary>
    /// 
    /// </summary>
    [Description("fa-chevron-right")]
    chevron,
    /// <summary>
    /// 
    /// </summary>
    [Description("fa-comment")]
    comment,
    /// <summary>
    /// 
    /// </summary>
    [Description("fa-star")]
    star,
    /// <summary>
    /// 
    /// </summary>
    [Description("fa-user")]
    user,
    /// <summary>
    /// 
    /// </summary>
    [Description("fa-cog")]
    cog
}
