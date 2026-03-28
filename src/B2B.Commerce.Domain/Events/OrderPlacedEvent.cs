namespace B2B.Commerce.Domain.Events;

public record OrderPlacedEvent : IDomainEvent
{
    public Guid OrderId { get; init; }
    public Guid? CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public int ItemCount { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
