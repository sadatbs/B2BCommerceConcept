using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Interfaces;
using B2B.Commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly CommerceDbContext _context;

    public ProductRepository(CommerceDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var normalizedSku = sku.Trim().ToUpperInvariant();
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Sku == normalizedSku, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Products.CountAsync(cancellationToken);

        var items = await _context.Products
            .OrderBy(p => p.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await GetByIdAsync(id, cancellationToken);
        if (product is not null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        var normalizedSku = sku.Trim().ToUpperInvariant();
        return await _context.Products
            .AnyAsync(p => p.Sku == normalizedSku, cancellationToken);
    }
}
