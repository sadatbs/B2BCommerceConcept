namespace B2B.Commerce.Contracts.Carts;

public record CreateCartRequest
{
    public required Guid UserId { get; init; }
    public required Guid CustomerId { get; init; }
}
