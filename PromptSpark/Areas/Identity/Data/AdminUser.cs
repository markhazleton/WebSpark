namespace PromptSpark.Areas.Identity.Data;

// Add profile data for application users by adding properties to the AdminUser class
public class AdminUser : IdentityUser
{
    public string FirstName { get; set; } = "First";
    public string LastName { get; set; } = "Last";
    public byte[]? ProfilePicture { get; set; }

}

