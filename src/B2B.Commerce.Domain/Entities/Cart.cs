namespace B2B.Commerce.Domain.Entities;

public class Cart : AggregateRoot
{
    private readonly List<CartItem> _items = new();

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }       // owner — individual buyer
    public Guid CustomerId { get; private set; }   // company context (for pricing, not ownership)
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    private Cart() { } // EF Core

    public static Cart Create(Guid userId, Guid customerId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required", nameof(userId));
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required", nameof(customerId));

        return new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = CartItem.Create(Id, productId, quantity);
            _items.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new InvalidOperationException($"Product {productId} not found in cart");

        if (quantity <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.UpdateQuantity(quantity);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new InvalidOperationException($"Product {productId} not found in cart");

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsEmpty => _items.Count == 0;
}
