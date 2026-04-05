namespace B2B.Commerce.Contracts.Pricing;

public record TierPriceDto
{
    public required Guid ProductId { get; init; }
    public required string Sku { get; init; }
    public required string ProductName { get; init; }
    public decimal ListPrice { get; init; }
    public decimal TierPrice { get; init; }
    public decimal Discount { get; init; }
}
