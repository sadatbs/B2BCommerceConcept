using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cart?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Cart> AddAsync(Cart cart, CancellationToken cancellationToken = default);
    void Update(Cart cart);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
