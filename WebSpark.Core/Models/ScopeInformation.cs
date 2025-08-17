namespace WebSpark.Core.Models;

/// <summary>
/// 
/// </summary>
public class ScopeInformation : Interfaces.IScopeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public ScopeInformation()
    {
        var entryAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;
        HostScopeInfo = new Dictionary<string, string>
        {
            {"MachineName", Environment.MachineName },
            {"EntryPoint", entryAssemblyName }
        };
    }

    /// <summary>
    /// 
    /// </summary>
    public Dictionary<string, string> HostScopeInfo { get; }
}
