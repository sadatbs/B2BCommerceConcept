namespace B2B.Commerce.Contracts.Requisitions;

public record RequisitionLineItemDto
{
    public required Guid Id { get; init; }
    public required Guid ProductId { get; init; }
    public required string Sku { get; init; }
    public required string ProductName { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}
