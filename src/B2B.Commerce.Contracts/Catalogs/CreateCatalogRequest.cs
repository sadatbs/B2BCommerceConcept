namespace B2B.Commerce.Contracts.Catalogs;

public record CreateCatalogRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
