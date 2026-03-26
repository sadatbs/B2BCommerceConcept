using B2B.Commerce.Contracts.Catalogs;
using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Api.Mapping;

public static class CatalogMappingExtensions
{
    public static CatalogDto ToDto(this Catalog catalog)
    {
        return new CatalogDto
        {
            Id = catalog.Id,
            Name = catalog.Name,
            Description = catalog.Description,
            CreatedAt = catalog.CreatedAt,
            ProductCount = catalog.CatalogProducts.Count
        };
    }

    public static CatalogDto ToDto(this Catalog catalog, int productCount)
    {
        return new CatalogDto
        {
            Id = catalog.Id,
            Name = catalog.Name,
            Description = catalog.Description,
            CreatedAt = catalog.CreatedAt,
            ProductCount = productCount
        };
    }

    public static CatalogDetailDto ToDetailDto(this Catalog catalog)
    {
        return new CatalogDetailDto
        {
            Id = catalog.Id,
            Name = catalog.Name,
            Description = catalog.Description,
            CreatedAt = catalog.CreatedAt,
            Products = catalog.CatalogProducts
                .Select(cp => cp.Product.ToDto())
                .ToList()
        };
    }

    public static IReadOnlyList<CatalogDto> ToDtos(this IEnumerable<Catalog> catalogs)
    {
        return catalogs.Select(c => c.ToDto()).ToList();
    }

    public static Catalog ToEntity(this CreateCatalogRequest request)
    {
        return Catalog.Create(request.Name, request.Description);
    }
}
