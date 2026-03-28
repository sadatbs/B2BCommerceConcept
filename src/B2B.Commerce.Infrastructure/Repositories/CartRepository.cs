using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Interfaces;
using B2B.Commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CommerceDbContext _context;

    public CartRepository(CommerceDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cart?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
    }

    public async Task<Cart> AddAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await _context.Carts.AddAsync(cart, cancellationToken);
        return cart;
    }

    public void Update(Cart cart)
    {
        // Snapshot of what was actually loaded from the DB for this cart.
        // DbSet.Local does NOT trigger DetectChanges (safe to call before explicit Adds).
        var persistedIds = _context.CartItems.Local
            .Where(ci => ci.CartId == cart.Id)
            .Select(ci => ci.Id)
            .ToHashSet();

        var currentIds = cart.Items.Select(i => i.Id).ToHashSet();

        // Items removed from the aggregate → Deleted → DELETE
        foreach (var removed in _context.CartItems.Local
            .Where(ci => ci.CartId == cart.Id && !currentIds.Contains(ci.Id))
            .ToList())
        {
            _context.CartItems.Remove(removed);
        }

        // Items new to the aggregate → Added → INSERT
        // Must run BEFORE any call that triggers DetectChanges or relationship fixup
        // (which would re-track new items as Unchanged and block the INSERT).
        foreach (var item in cart.Items.Where(i => !persistedIds.Contains(i.Id)))
            _context.CartItems.Add(item);

        // Cart scalar changes (UpdatedAt, etc.): the Cart was loaded in this scope and
        // is already snapshot-tracked. SaveChanges' internal DetectChanges will detect
        // UpdatedAt changed and issue UPDATE — no explicit Entry() call needed.
        // Avoiding Entry(cart) here prevents the relationship-fixup path that would
        // walk _items and re-track new CartItems with incorrect state.

        // Existing CartItems with changed Quantity are also snapshot-tracked — the
        // same SaveChanges DetectChanges pass handles them automatically.
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cart = await GetByIdAsync(id, cancellationToken);
        if (cart is not null)
        {
            _context.Carts.Remove(cart);
        }
    }
}
