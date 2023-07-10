using Microsoft.AspNetCore.Identity;

namespace ControlSpark.WebMvc.Areas.Identity.Data;
// Add profile data for application users by adding properties to the ControlSparkUser class
public class ControlSparkUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public byte[]? ProfilePicture { get; set; }

}

