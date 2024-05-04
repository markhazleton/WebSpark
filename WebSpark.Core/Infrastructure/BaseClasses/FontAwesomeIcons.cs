
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
        var attributes = field.GetCustomAttributes(false);

        // Description is in a hidden Attribute class called DisplayAttribute
        // Not to be confused with DisplayNameAttribute
        dynamic displayAttribute = null;

        if (attributes.Any())
        {
            displayAttribute = attributes.ElementAt(0);
        }

        // return description
        return displayAttribute?.Description ?? "Description Not Found";
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
