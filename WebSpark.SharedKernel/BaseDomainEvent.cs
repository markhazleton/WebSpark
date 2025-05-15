using MediatR;

namespace WebSpark.SharedKernel;

/// <summary>
/// Base class for domain events, compatible with MediatR for event-driven architecture.
/// </summary>
public abstract class BaseDomainEvent : INotification
{
    /// <summary>
    /// Gets the date and time when the event occurred (in UTC).
    /// </summary>
    public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;
}
