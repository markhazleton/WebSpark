using WebSpark.Domain.Interfaces;

namespace WebSpark.Domain.Models;

/// <summary>
/// 
/// </summary>
public class ScopeInformation : IScopeInformation
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
