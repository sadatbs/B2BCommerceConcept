namespace B2B.Commerce.Contracts.Carts;

public record AddToCartRequest
{
    public required Guid ProductId { get; init; }
    public int Quantity { get; init; } = 1;
}
