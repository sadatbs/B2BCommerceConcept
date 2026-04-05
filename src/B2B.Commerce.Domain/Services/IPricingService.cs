namespace B2B.Commerce.Domain.Services;

public interface IPricingService
{
    /// <summary>
    /// Get the price for a product for a specific customer.
    /// Returns tier price if customer has a tier with price set, otherwise list price.
    /// </summary>
    Task<decimal> GetPriceAsync(Guid productId, Guid? customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get prices for multiple products for a specific customer.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetPricesAsync(IEnumerable<Guid> productIds, Guid? customerId, CancellationToken cancellationToken = default);
}
