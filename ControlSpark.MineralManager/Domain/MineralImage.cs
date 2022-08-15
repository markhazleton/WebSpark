namespace ControlSpark.MineralManager.Domain
{

    public class MineralImage : ICollectionItemImage
    {
        public int CollectionItemID { get; set; }
        public int CollectionItemImageID { get; set; }
        public int DisplayOrder { get; set; }
        public string ImageDS { get; set; }
        public string ImageFileNM { get; set; }
        public string ImageNM { get; set; }
        public string ImageType { get; set; }
        public DateTime ModifiedDT { get; set; }
        public int ModifiedID { get; set; }
    }
}