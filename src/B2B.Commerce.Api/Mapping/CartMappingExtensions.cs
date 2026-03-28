using B2B.Commerce.Contracts.Carts;
using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Api.Mapping;

public static class CartMappingExtensions
{
    public static CartDto ToDto(this Cart cart)
    {
        var items = cart.Items.Select(i => i.ToDto()).ToList();

        return new CartDto
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Items = items,
            Subtotal = items.Sum(i => i.LineTotal),
            ItemCount = items.Sum(i => i.Quantity),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }

    public static CartItemDto ToDto(this CartItem item)
    {
        return new CartItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            Sku = item.Product.Sku,
            ProductName = item.Product.Name,
            UnitPrice = item.Product.Price,
            Quantity = item.Quantity,
            LineTotal = item.Product.Price * item.Quantity
        };
    }
}
