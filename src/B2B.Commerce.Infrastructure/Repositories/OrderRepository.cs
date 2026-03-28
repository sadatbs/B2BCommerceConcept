using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Interfaces;
using B2B.Commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly CommerceDbContext _context;

    public OrderRepository(CommerceDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        int skip, int take, CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Orders.CountAsync(cancellationToken);
        var items = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetByCustomerIdPagedAsync(
        Guid customerId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.Where(o => o.CustomerId == customerId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        return order;
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }
}
