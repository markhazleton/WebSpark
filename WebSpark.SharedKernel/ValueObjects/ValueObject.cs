using System;

namespace WebSpark.SharedKernel.ValueObjects
{
    /// <summary>
    /// Base class for value objects to standardize equality and immutability.
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;
            return Equals(obj as ValueObject);
        }

        public bool Equals(ValueObject? other)
        {
            if (other is null)
                return false;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                    HashCode.Combine(current, obj != null ? obj.GetHashCode() : 0));
        }

        protected abstract IEnumerable<object?> GetEqualityComponents();
    }
}
