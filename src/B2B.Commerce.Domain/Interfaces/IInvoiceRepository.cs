using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Domain.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, CancellationToken cancellationToken = default);
}
