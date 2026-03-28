namespace B2B.Commerce.Domain.Events;

public interface IEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
