namespace B2B.Commerce.Contracts.Pricing;

public record PriceTierDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public int ProductCount { get; init; }
    public required DateTime CreatedAt { get; init; }
}
