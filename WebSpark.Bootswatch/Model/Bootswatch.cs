namespace WebSpark.Bootswatch.Model;

/// <summary>
/// BootswatchResponse Api Response Model
/// </summary>
public class BootswatchResponse
{
#pragma warning disable IDE1006 // Naming Styles
    /// <summary>
    /// 
    /// </summary>
    public string? version { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List<BootswatchStyle>? themes { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}
