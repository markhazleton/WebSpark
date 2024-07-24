namespace WebSpark.Core.Interfaces;

/// <summary>
///  Scope Information - For Logging
/// </summary>
public interface IScopeInformation
{
    /// <summary>
    /// 
    /// </summary>
    Dictionary<string, string> HostScopeInfo { get; }
}
