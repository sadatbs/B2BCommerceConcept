using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Interfaces;
using B2B.Commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Infrastructure.Repositories;

public class CatalogRepository : ICatalogRepository
{
    private readonly CommerceDbContext _context;

    public CatalogRepository(CommerceDbContext context)
    {
        _context = context;
    }

    public async Task<Catalog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Catalogs
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Catalog?> GetByIdWithProductsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Catalogs
            .Include(c => c.CatalogProducts)
                .ThenInclude(cp => cp.Product)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Catalog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Catalogs
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Catalog> Items, int TotalCount)> GetPagedAsync(
        int skip, int take, CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Catalogs.CountAsync(cancellationToken);
        var items = await _context.Catalogs
            .OrderBy(c => c.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<Catalog> AddAsync(Catalog catalog, CancellationToken cancellationToken = default)
    {
        await _context.Catalogs.AddAsync(catalog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return catalog;
    }

    public async Task UpdateAsync(Catalog catalog, CancellationToken cancellationToken = default)
    {
        _context.Catalogs.Update(catalog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var catalog = await GetByIdAsync(id, cancellationToken);
        if (catalog is not null)
        {
            _context.Catalogs.Remove(catalog);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Catalogs.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetProductsInCatalogAsync(
        Guid catalogId, CancellationToken cancellationToken = default)
    {
        return await _context.CatalogProducts
            .Where(cp => cp.CatalogId == catalogId)
            .OrderBy(cp => cp.Product.Name)
            .Select(cp => cp.Product)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetProductsInCatalogPagedAsync(
        Guid catalogId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _context.CatalogProducts
            .Where(cp => cp.CatalogId == catalogId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(cp => cp.Product.Name)
            .Skip(skip)
            .Take(take)
            .Select(cp => cp.Product)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddProductToCatalogAsync(
        Guid catalogId, Guid productId, CancellationToken cancellationToken = default)
    {
        var catalogProduct = CatalogProduct.Create(catalogId, productId);
        await _context.CatalogProducts.AddAsync(catalogProduct, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddProductsToCatalogAsync(
        Guid catalogId, IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        var catalogProducts = productIds
            .Select(productId => CatalogProduct.Create(catalogId, productId))
            .ToList();

        await _context.CatalogProducts.AddRangeAsync(catalogProducts, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveProductFromCatalogAsync(
        Guid catalogId, Guid productId, CancellationToken cancellationToken = default)
    {
        var catalogProduct = await _context.CatalogProducts
            .FirstOrDefaultAsync(cp => cp.CatalogId == catalogId && cp.ProductId == productId, cancellationToken);

        if (catalogProduct is not null)
        {
            _context.CatalogProducts.Remove(catalogProduct);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsProductInCatalogAsync(
        Guid catalogId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.CatalogProducts
            .AnyAsync(cp => cp.CatalogId == catalogId && cp.ProductId == productId, cancellationToken);
    }
}
