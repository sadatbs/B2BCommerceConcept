using B2B.Commerce.Contracts.Invoices;
using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Enums;

namespace B2B.Commerce.Api.Mapping;

public static class InvoiceMappingExtensions
{
    public static InvoiceDto ToDto(this Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            TotalAmount = invoice.TotalAmount,
            IssuedAt = invoice.IssuedAt,
            DueDate = invoice.DueDate,
            Status = invoice.Status.ToString(),
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt
        };
    }

    public static IReadOnlyList<InvoiceDto> ToDtos(this IEnumerable<Invoice> invoices)
    {
        return invoices.Select(i => i.ToDto()).ToList();
    }

    public static Invoice ToEntity(this CreateInvoiceRequest request)
    {
        return Invoice.Create(
            request.InvoiceNumber,
            request.TotalAmount,
            request.IssuedAt,
            request.DueDate
        );
    }

    public static InvoiceStatus ToInvoiceStatus(this string status)
    {
        if (!Enum.TryParse<InvoiceStatus>(status, ignoreCase: true, out var parsed))
            throw new ArgumentException($"Invalid invoice status: {status}", nameof(status));
        return parsed;
    }
}
