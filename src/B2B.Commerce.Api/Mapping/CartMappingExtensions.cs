using B2B.Commerce.Contracts.Carts;
using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Services;

namespace B2B.Commerce.Api.Mapping;

public static class CartMappingExtensions
{
    public static async Task<CartDto> ToDtoAsync(this Cart cart, IPricingService pricingService, CancellationToken cancellationToken = default)
    {
        var productIds = cart.Items.Select(i => i.ProductId).ToList();
        var prices = await pricingService.GetPricesAsync(productIds, cart.CustomerId, cancellationToken);

        var items = cart.Items.Select(i => i.ToDto(prices.GetValueOrDefault(i.ProductId, i.Product.Price))).ToList();

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            CustomerId = cart.CustomerId,
            Items = items,
            Subtotal = items.Sum(i => i.LineTotal),
            ItemCount = items.Sum(i => i.Quantity),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }

    public static CartItemDto ToDto(this CartItem item, decimal unitPrice)
    {
        return new CartItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            Sku = item.Product.Sku,
            ProductName = item.Product.Name,
            UnitPrice = unitPrice,
            Quantity = item.Quantity,
            LineTotal = unitPrice * item.Quantity
        };
    }

    // Fallback for empty cart or cases without items loaded
    public static CartDto ToDto(this Cart cart)
    {
        var items = cart.Items.Select(i => i.ToDto(i.Product.Price)).ToList();

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            CustomerId = cart.CustomerId,
            Items = items,
            Subtotal = items.Sum(i => i.LineTotal),
            ItemCount = items.Sum(i => i.Quantity),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }
}
