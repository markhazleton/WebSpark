using System.ComponentModel.DataAnnotations;

namespace ControlSpark.MineralManager.Entities;

public partial class Mineral
{
    [Display(Name = "Mineral ID")]
    public int MineralId { get; set; }

    [Display(Name = "Mineral Name")]
    public string MineralNm { get; set; } = null!;

    [Display(Name = "Mineral Description")]
    [DataType(DataType.MultilineText)]
    [MaxLength(5000)]
    [MinLength(0)]
    [UIHint("TextArea")]
    public string? MineralDs { get; set; }

    [Display(Name = "Wikipedia URL")]
    [DataType(DataType.Url)]
    [DisplayFormat(HtmlEncode = false)]
    public string? WikipediaUrl { get; set; }

    [Display(Name = "Modified ID")]
    public int ModifiedId { get; set; }

    [Display(Name = "Modified Date")]
    public DateTime ModifiedDt { get; set; }

    public virtual ICollection<CollectionItemMineral>? CollectionItemMinerals { get; set; }
    public virtual ICollection<CollectionItem>? CollectionItems { get; set; }
}
