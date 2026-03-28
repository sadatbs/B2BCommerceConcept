using B2B.Commerce.Domain.Enums;
using B2B.Commerce.Domain.Events;

namespace B2B.Commerce.Domain.Entities;

public class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public Guid? CustomerId { get; private set; }
    public string? PurchaseOrderNumber { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // EF Core

    public static Order CreateFromCart(Cart cart, Func<Guid, Product?> productLookup, string? purchaseOrderNumber = null)
    {
        if (cart.IsEmpty)
            throw new InvalidOperationException("Cannot create order from empty cart");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = cart.CustomerId,
            PurchaseOrderNumber = purchaseOrderNumber?.Trim(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var cartItem in cart.Items)
        {
            var product = productLookup(cartItem.ProductId)
                ?? throw new InvalidOperationException($"Product {cartItem.ProductId} not found");

            var orderItem = OrderItem.CreateFromCartItem(order.Id, cartItem, product);
            order._items.Add(orderItem);
        }

        order.TotalAmount = order._items.Sum(i => i.LineTotal);

        order.AddDomainEvent(new OrderPlacedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            ItemCount = order._items.Count
        });

        return order;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm order in {Status} status");

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot complete order in {Status} status");

        Status = OrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}
