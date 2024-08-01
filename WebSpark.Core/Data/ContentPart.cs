namespace WebSpark.Core.Data
{
    public partial class ContentPart : BaseEntity
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Content { get; set; }
        public virtual ICollection<Keyword> Keywords { get; set; } = [];
    }
}
