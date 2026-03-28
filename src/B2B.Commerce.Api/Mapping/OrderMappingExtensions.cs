using B2B.Commerce.Contracts.Orders;
using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Api.Mapping;

public static class OrderMappingExtensions
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            PurchaseOrderNumber = order.PurchaseOrderNumber,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ItemCount = order.Items.Sum(i => i.Quantity),
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt,
            CompletedAt = order.CompletedAt
        };
    }

    public static OrderDetailDto ToDetailDto(this Order order)
    {
        return new OrderDetailDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            PurchaseOrderNumber = order.PurchaseOrderNumber,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => i.ToDto()).ToList(),
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt,
            CompletedAt = order.CompletedAt
        };
    }

    public static OrderItemDto ToDto(this OrderItem item)
    {
        return new OrderItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            Sku = item.Sku,
            ProductName = item.ProductName,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            LineTotal = item.LineTotal
        };
    }

    public static IReadOnlyList<OrderDto> ToDtos(this IEnumerable<Order> orders)
    {
        return orders.Select(o => o.ToDto()).ToList();
    }
}
