namespace B2B.Commerce.Domain.Entities;

public class CatalogProduct
{
    public Guid CatalogId { get; private set; }
    public Catalog Catalog { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public DateTime AddedAt { get; private set; }

    private CatalogProduct() { }

    public static CatalogProduct Create(Guid catalogId, Guid productId)
    {
        return new CatalogProduct
        {
            CatalogId = catalogId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        };
    }
}
