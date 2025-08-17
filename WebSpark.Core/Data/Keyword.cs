namespace WebSpark.Core.Data
{
    public partial class Keyword : BaseEntity
    {
        public required string Name { get; set; } = string.Empty;
        public required string Description { get; set; } = string.Empty;
        public virtual ICollection<Menu> Menus { get; set; } = [];
        public virtual ICollection<ContentPart> ContentParts { get; set; } = [];
    }
}
