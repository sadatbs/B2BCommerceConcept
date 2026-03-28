namespace B2B.Commerce.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
