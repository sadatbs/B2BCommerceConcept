namespace B2B.Commerce.Contracts.Carts;

public record UpdateCartItemRequest
{
    public int Quantity { get; init; }
}
