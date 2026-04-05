namespace B2B.Commerce.Contracts.Customers;

public record AssignPriceTierRequest
{
    public Guid? PriceTierId { get; init; }
}
