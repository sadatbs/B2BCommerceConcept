namespace B2B.Commerce.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string? Category { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<CatalogProduct> CatalogProducts { get; private set; } = new List<CatalogProduct>();

    private Product() { }

    public static Product Create(string sku, string name, decimal price, string? description = null, string? category = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required", nameof(sku));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        return new Product
        {
            Id = Guid.NewGuid(),
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Price = price,
            Category = category?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative", nameof(newPrice));
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description, string? category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        Name = name.Trim();
        Description = description?.Trim();
        Category = category?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
