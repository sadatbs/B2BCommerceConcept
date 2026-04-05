namespace B2B.Commerce.Contracts.Pricing;

public record PriceTierDetailDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required IReadOnlyList<TierPriceDto> Prices { get; init; }
    public required DateTime CreatedAt { get; init; }
}
