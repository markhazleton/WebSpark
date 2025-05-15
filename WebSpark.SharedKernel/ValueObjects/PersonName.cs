namespace WebSpark.SharedKernel.ValueObjects;

/// <summary>
/// Represents a person's name as a value object.
/// </summary>
public sealed class PersonName : ValueObject
{
    /// <summary>
    /// First name of the person.
    /// </summary>
    public string FirstName { get; init; }
    /// <summary>
    /// Last name of the person.
    /// </summary>
    public string LastName { get; init; }

    /// <summary>
    /// Creates a new instance of <see cref="PersonName"/>.
    /// </summary>
    /// <param name="first">First name.</param>
    /// <param name="last">Last name.</param>
    public PersonName(string first, string last)
    {
        FirstName = first;
        LastName = last;
    }

    /// <summary>
    /// Returns the full name.
    /// </summary>
    public string FullName => $"{FirstName.Trim()} {LastName.Trim()}";
    /// <summary>
    /// Returns the name in reverse order.
    /// </summary>
    public string ReverseName => $"{LastName.Trim()}, {FirstName.Trim()}";
    /// <summary>
    /// Returns initials.
    /// </summary>
    public string SingleInitials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}";
    /// <summary>
    /// Returns complex initials.
    /// </summary>
    public string ComplexInitials =>
        $"{string.Concat(FirstName, "__").Substring(0, 3)}" +
        $"{string.Concat(LastName, "__").Substring(0, 3)}";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }
}
