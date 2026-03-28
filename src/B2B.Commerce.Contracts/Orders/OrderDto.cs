namespace B2B.Commerce.Contracts.Orders;

public record OrderDto
{
    public required Guid Id { get; init; }
    public Guid? CustomerId { get; init; }
    public string? PurchaseOrderNumber { get; init; }
    public required string Status { get; init; }
    public decimal TotalAmount { get; init; }
    public int ItemCount { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
