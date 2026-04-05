namespace B2B.Commerce.Contracts.Pricing;

public record CreatePriceTierRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
