namespace ControlSpark.WebMvc.Areas.Identity.Data;

public abstract class BaseEntity
{
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

