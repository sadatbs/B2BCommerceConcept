using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Interfaces;
using B2B.Commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly CommerceDbContext _context;

    public InvoiceRepository(CommerceDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        var normalized = invoiceNumber.Trim().ToUpperInvariant();
        return await _context.Invoices
            .FirstOrDefaultAsync(i => i.InvoiceNumber == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetPagedAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Invoices.CountAsync(cancellationToken);

        var items = await _context.Invoices
            .OrderByDescending(i => i.IssuedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(id, cancellationToken);
        if (invoice is not null)
        {
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        var normalized = invoiceNumber.Trim().ToUpperInvariant();
        return await _context.Invoices
            .AnyAsync(i => i.InvoiceNumber == normalized, cancellationToken);
    }
}
