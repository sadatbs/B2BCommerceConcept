using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Domain.Interfaces;

public interface IPriceTierRepository
{
    Task<PriceTier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PriceTier?> GetByIdWithPricesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriceTier>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PriceTier> AddAsync(PriceTier priceTier, CancellationToken cancellationToken = default);
    void Update(PriceTier priceTier);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    // TierPrice management
    Task<TierPrice?> GetTierPriceAsync(Guid tierId, Guid productId, CancellationToken cancellationToken = default);
    Task SetTierPriceAsync(Guid tierId, Guid productId, decimal price, CancellationToken cancellationToken = default);
    Task RemoveTierPriceAsync(Guid tierId, Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TierPrice>> GetTierPricesAsync(Guid tierId, CancellationToken cancellationToken = default);
}
