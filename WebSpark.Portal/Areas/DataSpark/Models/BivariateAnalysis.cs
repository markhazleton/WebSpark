﻿namespace WebSpark.Portal.Areas.DataSpark.Models
{
    public class BivariateAnalysis
    {
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public List<string> Observations { get; set; } = [];
        public List<string> VisualizationRecommendations { get; set; } = [];
        public double InsightScore { get; set; } // Score indicating the probability of gaining good insights
    }
}