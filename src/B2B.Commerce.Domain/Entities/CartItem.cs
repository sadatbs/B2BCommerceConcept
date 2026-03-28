namespace B2B.Commerce.Domain.Entities;

public class CartItem
{
    public Guid Id { get; private set; }
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime AddedAt { get; private set; }

    // Navigation
    public Cart Cart { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private CartItem() { } // EF Core

    internal static CartItem Create(Guid cartId, Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        // Do not set Id — leave it as Guid.Empty so EF recognises this as a new
        // entity (ValueGeneratedOnAdd default for Guid PKs). EF generates the Guid
        // client-side when the entity is first tracked as Added during SaveChanges.
        return new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = quantity,
            AddedAt = DateTime.UtcNow
        };
    }

    internal void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        Quantity = quantity;
    }
}
