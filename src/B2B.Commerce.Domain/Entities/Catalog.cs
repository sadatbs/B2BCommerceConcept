namespace B2B.Commerce.Domain.Entities;

public class Catalog
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<CatalogProduct> CatalogProducts { get; private set; } = new List<CatalogProduct>();

    private Catalog() { }

    public static Catalog Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        return new Catalog
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
    }
}
