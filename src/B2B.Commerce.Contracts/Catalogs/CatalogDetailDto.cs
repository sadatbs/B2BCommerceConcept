using B2B.Commerce.Contracts.Products;

namespace B2B.Commerce.Contracts.Catalogs;

public record CatalogDetailDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required IReadOnlyList<ProductDto> Products { get; init; }
}
