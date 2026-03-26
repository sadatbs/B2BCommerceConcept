namespace B2B.Commerce.Contracts.Products;

public record ProductDto
{
    public required Guid Id { get; init; }
    public required string Sku { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required decimal Price { get; init; }
    public string? Category { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
