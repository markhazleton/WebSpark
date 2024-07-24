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
        HostScopeInfo = new Dictionary<string, string>
        {
            {"MachineName", Environment.MachineName },
            {"EntryPoint", Assembly.GetEntryAssembly().GetName().Name }
        };
    }

    /// <summary>
    /// 
    /// </summary>
    public Dictionary<string, string> HostScopeInfo { get; }
}
