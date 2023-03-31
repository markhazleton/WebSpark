
namespace ControlSpark.Core.Infrastructure.Middleware;

/// <summary>
/// ApiError
/// </summary>
public class ApiError
{
    /// <summary>
    /// SiteTemplate 
    /// </summary>
    public string? Code { get; set; }
    /// <summary>
    /// Detail 
    /// </summary>
    public string? Detail { get; set; }
    /// <summary>
    /// Id 
    /// </summary>
    public string? Id { get; set; }
    /// <summary>
    /// Links 
    /// </summary>
    public string? Links { get; set; }
    /// <summary>
    /// Status 
    /// </summary>
    public short Status { get; set; }
    /// <summary>
    /// Title 
    /// </summary>
    public string? Title { get; set; }
}
