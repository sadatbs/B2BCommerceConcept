namespace B2B.Commerce.Contracts.Products;

public record UpdateProductRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
}
