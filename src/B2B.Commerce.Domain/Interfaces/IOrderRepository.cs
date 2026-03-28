using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetByCustomerIdPagedAsync(Guid customerId, int skip, int take, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    void Update(Order order);
}
