using B2B.Commerce.Contracts.Products;
using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Api.Mapping;

public static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public static IReadOnlyList<ProductDto> ToDtos(this IEnumerable<Product> products)
    {
        return products.Select(p => p.ToDto()).ToList();
    }

    public static Product ToEntity(this CreateProductRequest request)
    {
        return Product.Create(
            request.Sku,
            request.Name,
            request.Price,
            request.Description,
            request.Category
        );
    }
}
