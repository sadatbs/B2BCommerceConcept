namespace B2B.Commerce.Contracts.Orders;

public record CreateOrderRequest
{
    public required Guid CartId { get; init; }
    public string? PurchaseOrderNumber { get; init; }
}
