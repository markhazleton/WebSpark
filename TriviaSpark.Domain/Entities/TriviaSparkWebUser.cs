namespace TriviaSpark.Domain.Entities;

// Add profile data for application users by adding properties to the TriviaSparkWebUser class
public class TriviaSparkWebUser
{
    public string UserName { get; set; }
    public string Id { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public byte[]? ProfilePicture { get; set; }
    public virtual ICollection<Match> Matches { get; set; }
}
