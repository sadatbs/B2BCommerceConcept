using B2B.Commerce.Domain.Events;
using Microsoft.Extensions.Logging;

namespace B2B.Commerce.Infrastructure.Events;

public class InMemoryEventDispatcher : IEventDispatcher
{
    private readonly ILogger<InMemoryEventDispatcher> _logger;

    public InMemoryEventDispatcher(ILogger<InMemoryEventDispatcher> logger)
    {
        _logger = logger;
    }

    public Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            _logger.LogInformation(
                "Domain event dispatched: {EventType} at {OccurredAt}",
                @event.GetType().Name,
                @event.OccurredAt);

            if (@event is OrderPlacedEvent orderPlaced)
            {
                _logger.LogInformation(
                    "Order {OrderId} placed. Total: {Total:C}, Items: {ItemCount}",
                    orderPlaced.OrderId,
                    orderPlaced.TotalAmount,
                    orderPlaced.ItemCount);
            }
        }

        return Task.CompletedTask;
    }
}
