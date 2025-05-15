using MediatR;

namespace WebSpark.SharedKernel;

/// <summary>
/// Helper service to dispatch domain events using MediatR.
/// </summary>
public class DomainEventDispatcher
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventDispatcher"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator instance.</param>
    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Publishes all domain events for the given entity and clears the event list.
    /// </summary>
    /// <param name="entity">The entity whose events to dispatch.</param>
    public async Task DispatchEventsAsync(BaseEntity<object> entity)
    {
        var events = entity.Events.ToList();
        entity.Events.Clear();
        foreach (var domainEvent in events)
        {
            await _mediator.Publish(domainEvent);
        }
    }
}
