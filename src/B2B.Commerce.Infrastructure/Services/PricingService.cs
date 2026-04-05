using B2B.Commerce.Domain.Services;
using B2B.Commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Infrastructure.Services;

public class PricingService : IPricingService
{
    private readonly CommerceDbContext _context;

    public PricingService(CommerceDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetPriceAsync(Guid productId, Guid? customerId, CancellationToken cancellationToken = default)
    {
        var prices = await GetPricesAsync(new[] { productId }, customerId, cancellationToken);
        return prices.TryGetValue(productId, out var price) ? price : 0;
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetPricesAsync(
        IEnumerable<Guid> productIds,
        Guid? customerId,
        CancellationToken cancellationToken = default)
    {
        var productIdList = productIds.ToList();

        // Get list prices for all products
        var products = await _context.Products
            .Where(p => productIdList.Contains(p.Id))
            .Select(p => new { p.Id, p.Price })
            .ToDictionaryAsync(p => p.Id, p => p.Price, cancellationToken);

        // If no customer, return list prices
        if (customerId is null)
            return products;

        // Get customer's price tier
        var customer = await _context.Customers
            .Where(c => c.Id == customerId)
            .Select(c => new { c.PriceTierId })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer?.PriceTierId is null)
            return products;

        // Get tier prices for products that have them
        var tierPrices = await _context.TierPrices
            .Where(tp => tp.PriceTierId == customer.PriceTierId && productIdList.Contains(tp.ProductId))
            .ToDictionaryAsync(tp => tp.ProductId, tp => tp.Price, cancellationToken);

        // Merge: tier price if available, otherwise list price
        var result = new Dictionary<Guid, decimal>();
        foreach (var productId in productIdList)
        {
            if (tierPrices.TryGetValue(productId, out var tierPrice))
                result[productId] = tierPrice;
            else if (products.TryGetValue(productId, out var listPrice))
                result[productId] = listPrice;
        }

        return result;
    }
}
