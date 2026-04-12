namespace B2B.Commerce.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }

    // Snapshot at time of order
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    // Navigation
    public Order Order { get; private set; } = null!;

    public decimal LineTotal => UnitPrice * Quantity;

    private OrderItem() { } // EF Core

    internal static OrderItem CreateFromCartItem(Guid orderId, CartItem cartItem, Product product)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = product.Id,
            Sku = product.Sku,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = cartItem.Quantity
        };
    }

    internal static OrderItem CreateFromRequisitionLineItem(Guid orderId, RequisitionLineItem lineItem)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = lineItem.ProductId,
            Sku = lineItem.Sku,
            ProductName = lineItem.ProductName,
            UnitPrice = lineItem.UnitPrice,   // already frozen in Requisition
            Quantity = lineItem.Quantity
        };
    }
}
