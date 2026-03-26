namespace B2B.Commerce.Contracts.Catalogs;

public record AddProductsToCatalogRequest
{
    public required IReadOnlyList<Guid> ProductIds { get; init; }
}
