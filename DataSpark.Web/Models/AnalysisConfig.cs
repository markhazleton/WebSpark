﻿namespace DataSpark.Web.Models;


/// <summary>
/// Configuration class for customizing univariate analysis behavior.
/// </summary>
public class AnalysisConfig
{
    internal readonly long NonUniqueThresholdForHistograms = 20;
    internal readonly long NonUniqueThresholdForBoxPlots = 20;

    public double HighSkewnessThreshold { get; set; } = 1.0;
    public double ModerateSkewnessThreshold { get; set; } = 0.5;
    public int UniqueCountThreshold { get; set; } = 20;
    public double OutlierStdDevMultiplier { get; set; } = 3.0;
}
