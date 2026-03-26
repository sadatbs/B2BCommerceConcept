namespace B2B.Commerce.Contracts.Products;

public record CreateProductRequest
{
    public required string Sku { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required decimal Price { get; init; }
    public string? Category { get; init; }
}
