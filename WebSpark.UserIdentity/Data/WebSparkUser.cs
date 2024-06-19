using Microsoft.AspNetCore.Identity;

namespace WebSpark.Domain.User.Data;
/// <summary>
/// Add profile data for application users by adding properties to the WebSparkUser class
/// </summary>
public class WebSparkUser : IdentityUser
{

    /// <summary>
    /// First Name of User
    /// </summary>
    public string? FirstName { get; set; }
    /// <summary>
    /// Last Name of User
    /// </summary>
    public string? LastName { get; set; }
    /// <summary>
    /// Profile Picture of User
    /// </summary>
    public byte[]? ProfilePicture { get; set; }

}

