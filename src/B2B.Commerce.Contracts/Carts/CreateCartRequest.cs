namespace B2B.Commerce.Contracts.Carts;

public record CreateCartRequest
{
    public Guid? CustomerId { get; init; }
}
