namespace WebSpark.Core.Data
{
    public partial class ContentPart : BaseEntity
    {
        public required string Title { get; set; } = string.Empty;
        public required string Description { get; set; } = string.Empty;
        public required string Content { get; set; } = string.Empty;
        public virtual ICollection<Keyword> Keywords { get; set; } = [];
    }
}
