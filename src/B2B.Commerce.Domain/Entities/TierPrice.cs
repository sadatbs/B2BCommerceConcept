namespace B2B.Commerce.Domain.Entities;

public class TierPrice
{
    public Guid PriceTierId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Price { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public PriceTier PriceTier { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private TierPrice() { } // EF Core

    public static TierPrice Create(Guid priceTierId, Guid productId, decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        return new TierPrice
        {
            PriceTierId = priceTierId,
            ProductId = productId,
            Price = price,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }
}
