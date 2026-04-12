using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Enums;

namespace B2B.Commerce.Domain.Interfaces;

public interface IRequisitionRepository
{
    Task<Requisition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Requisition?> GetByIdWithLineItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Requisition> Items, int TotalCount)> GetByUserIdPagedAsync(
        Guid userId, int skip, int take, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Requisition> Items, int TotalCount)> GetByCustomerIdPagedAsync(
        Guid customerId, int skip, int take, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Requisition> Items, int TotalCount)> GetPendingApprovalPagedAsync(
        Guid customerId, int skip, int take, CancellationToken cancellationToken = default);
    Task<Requisition> AddAsync(Requisition requisition, CancellationToken cancellationToken = default);
    void Update(Requisition requisition);
}
