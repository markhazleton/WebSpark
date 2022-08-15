namespace ControlSpark.MineralManager.Domain;

public interface ICollectionItemImage
{
    int CollectionItemImageID { get; set; }
    int CollectionItemID { get; set; }
    string ImageType { get; set; }
    int DisplayOrder { get; set; }
    string ImageNM { get; set; }
    string ImageDS { get; set; }
    string ImageFileNM { get; set; }
    int ModifiedID { get; set; }
    DateTime ModifiedDT { get; set; }
}