using B2B.Commerce.Contracts.Pricing;
using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Api.Mapping;

public static class PriceTierMappingExtensions
{
    public static PriceTierDto ToDto(this PriceTier tier)
    {
        return new PriceTierDto
        {
            Id = tier.Id,
            Name = tier.Name,
            Description = tier.Description,
            ProductCount = tier.Prices.Count,
            CreatedAt = tier.CreatedAt
        };
    }

    public static PriceTierDetailDto ToDetailDto(this PriceTier tier)
    {
        return new PriceTierDetailDto
        {
            Id = tier.Id,
            Name = tier.Name,
            Description = tier.Description,
            Prices = tier.Prices.Select(p => p.ToDto()).ToList(),
            CreatedAt = tier.CreatedAt
        };
    }

    public static TierPriceDto ToDto(this TierPrice tierPrice)
    {
        var listPrice = tierPrice.Product.Price;
        var discount = listPrice > 0 ? (listPrice - tierPrice.Price) / listPrice * 100 : 0;

        return new TierPriceDto
        {
            ProductId = tierPrice.ProductId,
            Sku = tierPrice.Product.Sku,
            ProductName = tierPrice.Product.Name,
            ListPrice = listPrice,
            TierPrice = tierPrice.Price,
            Discount = Math.Round(discount, 2)
        };
    }

    public static IReadOnlyList<PriceTierDto> ToDtos(this IEnumerable<PriceTier> tiers)
    {
        return tiers.Select(t => t.ToDto()).ToList();
    }
}
