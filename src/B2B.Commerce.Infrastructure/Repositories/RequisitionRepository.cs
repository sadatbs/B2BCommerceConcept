using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Enums;
using B2B.Commerce.Domain.Interfaces;
using B2B.Commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Infrastructure.Repositories;

public class RequisitionRepository : IRequisitionRepository
{
    private readonly CommerceDbContext _context;

    public RequisitionRepository(CommerceDbContext context)
    {
        _context = context;
    }

    public async Task<Requisition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Requisitions
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Requisition?> GetByIdWithLineItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Requisitions
            .Include(r => r.LineItems)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Requisition> Items, int TotalCount)> GetByUserIdPagedAsync(
        Guid userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _context.Requisitions.Where(r => r.UserId == userId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.SubmittedAt)
            .Skip(skip).Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Requisition> Items, int TotalCount)> GetByCustomerIdPagedAsync(
        Guid customerId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _context.Requisitions.Where(r => r.CustomerId == customerId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.SubmittedAt)
            .Skip(skip).Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Requisition> Items, int TotalCount)> GetPendingApprovalPagedAsync(
        Guid customerId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _context.Requisitions
            .Where(r => r.CustomerId == customerId && r.Status == RequisitionStatus.Submitted);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(r => r.SubmittedAt)  // oldest first for approval queue
            .Skip(skip).Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<Requisition> AddAsync(Requisition requisition, CancellationToken cancellationToken = default)
    {
        await _context.Requisitions.AddAsync(requisition, cancellationToken);
        return requisition;
    }

    public void Update(Requisition requisition)
    {
        _context.Requisitions.Update(requisition);
    }
}
