﻿using System.Globalization;
using System.Reflection;

namespace InquirySpark.Domain.Models;

/// <summary>
/// ApplicationStatus
/// </summary>
public sealed class ApplicationStatus
{
    public ApplicationStatus(Assembly assembly, Dictionary<string, string> tests = null, List<string> messages = null, ServiceStatus status = ServiceStatus.Online)
    {
        BuildDate = GetBuildDate(assembly);
        BuildVersion = new BuildVersion(assembly?.GetName().Version);
        Tests = tests ?? [];
        Messages = messages ?? [];
        Status = status;
    }

    private DateTime GetBuildDate(Assembly assembly)
    {
        const string BuildVersionMetadataPrefix = "+build";
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix);
            if (index > 0)
            {
                value = value[(index + BuildVersionMetadataPrefix.Length)..];
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }
        }
        return DateTime.MinValue;
    }

    public DateTime BuildDate { get; }
    /// <summary>
    /// BuildVersion
    /// </summary>
    public BuildVersion BuildVersion { get; }
    /// <summary>
    /// Features
    /// </summary>
    public Dictionary<string, string> Features { get; } = [];
    /// <summary>
    /// Messages
    /// </summary>
    public List<string> Messages { get; } = [];
    /// <summary>
    /// Region
    /// </summary>
    public string Region { get; } = Environment.GetEnvironmentVariable("Region") ?? Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
    /// <summary>
    /// Status 
    /// </summary>
    public ServiceStatus Status { get; } = ServiceStatus.Online;
    /// <summary>
    /// Tests 
    /// </summary>
    public Dictionary<string, string> Tests { get; } = [];
}