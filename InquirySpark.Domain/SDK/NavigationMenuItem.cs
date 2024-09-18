
namespace InquirySpark.Domain.SDK;

public class NavigationMenuItem
{
    public int Id { get; set; }
    public int? SiteAppID { get; set; }
    public string MenuText { get; set; }
    public string TartgetPage { get; set; }
    public string GlyphName { get; set; }
    public int? MenuOrder { get; set; }
    public int? SiteRoleID { get; set; }
    public bool? ViewInMenu { get; set; }
    public bool IsSelected { get; set; }
    public string Css { get; set; }
}