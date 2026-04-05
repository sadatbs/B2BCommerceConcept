using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Interfaces;
using B2B.Commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Infrastructure.Repositories;

public class PriceTierRepository : IPriceTierRepository
{
    private readonly CommerceDbContext _context;

    public PriceTierRepository(CommerceDbContext context)
    {
        _context = context;
    }

    public async Task<PriceTier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PriceTiers
            .FirstOrDefaultAsync(pt => pt.Id == id, cancellationToken);
    }

    public async Task<PriceTier?> GetByIdWithPricesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PriceTiers
            .Include(pt => pt.Prices)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(pt => pt.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PriceTier>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PriceTiers
            .OrderBy(pt => pt.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<PriceTier> AddAsync(PriceTier priceTier, CancellationToken cancellationToken = default)
    {
        await _context.PriceTiers.AddAsync(priceTier, cancellationToken);
        return priceTier;
    }

    public void Update(PriceTier priceTier)
    {
        _context.PriceTiers.Update(priceTier);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tier = await GetByIdAsync(id, cancellationToken);
        if (tier is not null)
        {
            _context.PriceTiers.Remove(tier);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PriceTiers.AnyAsync(pt => pt.Id == id, cancellationToken);
    }

    public async Task<TierPrice?> GetTierPriceAsync(Guid tierId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.TierPrices
            .Include(tp => tp.Product)
            .FirstOrDefaultAsync(tp => tp.PriceTierId == tierId && tp.ProductId == productId, cancellationToken);
    }

    public async Task SetTierPriceAsync(Guid tierId, Guid productId, decimal price, CancellationToken cancellationToken = default)
    {
        var existing = await _context.TierPrices
            .FirstOrDefaultAsync(tp => tp.PriceTierId == tierId && tp.ProductId == productId, cancellationToken);

        if (existing is not null)
        {
            existing.UpdatePrice(price);
        }
        else
        {
            var tierPrice = TierPrice.Create(tierId, productId, price);
            await _context.TierPrices.AddAsync(tierPrice, cancellationToken);
        }
    }

    public async Task RemoveTierPriceAsync(Guid tierId, Guid productId, CancellationToken cancellationToken = default)
    {
        var tierPrice = await _context.TierPrices
            .FirstOrDefaultAsync(tp => tp.PriceTierId == tierId && tp.ProductId == productId, cancellationToken);

        if (tierPrice is not null)
        {
            _context.TierPrices.Remove(tierPrice);
        }
    }

    public async Task<IReadOnlyList<TierPrice>> GetTierPricesAsync(Guid tierId, CancellationToken cancellationToken = default)
    {
        return await _context.TierPrices
            .Include(tp => tp.Product)
            .Where(tp => tp.PriceTierId == tierId)
            .OrderBy(tp => tp.Product.Name)
            .ToListAsync(cancellationToken);
    }
}
