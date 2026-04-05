namespace B2B.Commerce.Contracts.Pricing;

public record SetTierPriceRequest
{
    public required Guid ProductId { get; init; }
    public decimal Price { get; init; }
}
