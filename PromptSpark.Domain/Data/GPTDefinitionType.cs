﻿using PromptSpark.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace PromptSpark.Domain.Data;

public class GPTDefinitionType
{
    [Key]
    public string DefinitionType { get; set; }
    public string Description { get; set; }
    public OutputType OutputType { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Updated { get; set; } = DateTime.Now;
}
