﻿namespace WebSpark.SharedKernel.ValueObjects;

public class PersonName
{
    private PersonName() { }
    public string FirstName { get; init; }
    public string LastName { get; init; }

    public PersonName(string first, string last)
    {
        FirstName = first;
        LastName = last;
    }
    public string FullName => $"{FirstName.Trim()} {LastName.Trim()}";
    public string ReverseName => $"{LastName.Trim()}, {FirstName.Trim()}";
    public string SingleInitials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}";
    public string ComplexInitials =>
        $"{string.Concat(FirstName, "__").Substring(0, 3)}" +
        $"{string.Concat(LastName, "__").Substring(0, 3)}";

    public override bool Equals(object? obj)
    {
        return obj is ValueObjects.PersonName name &&
               FirstName == name.FirstName &&
               LastName == name.LastName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FirstName, LastName);
    }
}
