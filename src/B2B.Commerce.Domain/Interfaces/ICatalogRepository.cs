using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Domain.Interfaces;

public interface ICatalogRepository
{
    Task<Catalog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Catalog?> GetByIdWithProductsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Catalog>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Catalog> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<Catalog> AddAsync(Catalog catalog, CancellationToken cancellationToken = default);
    Task UpdateAsync(Catalog catalog, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetProductsInCatalogAsync(Guid catalogId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetProductsInCatalogPagedAsync(Guid catalogId, int skip, int take, CancellationToken cancellationToken = default);
    Task AddProductToCatalogAsync(Guid catalogId, Guid productId, CancellationToken cancellationToken = default);
    Task AddProductsToCatalogAsync(Guid catalogId, IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
    Task RemoveProductFromCatalogAsync(Guid catalogId, Guid productId, CancellationToken cancellationToken = default);
    Task<bool> IsProductInCatalogAsync(Guid catalogId, Guid productId, CancellationToken cancellationToken = default);
}
