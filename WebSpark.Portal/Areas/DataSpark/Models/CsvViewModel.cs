﻿using Microsoft.Data.Analysis;

namespace WebSpark.Portal.Areas.DataSpark.Models;

public class CsvViewModel
{
    public List<string> AvailableCsvFiles { get; set; } = new List<string>();
    public List<BivariateAnalysis> BivariateAnalyses { get; set; } = [];
    public int ColumnCount { get; set; }
    public List<ColumnInfo> ColumnDetails { get; set; } = [];
    public DataFrame Description { get; set; } = new();
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public DataFrame Head { get; set; } = new();
    public DataFrame Info { get; internal set; } = new();
    public string Message { get; set; }
    public long RowCount { get; set; }
}

