using B2B.Commerce.Domain.Enums;

namespace B2B.Commerce.Domain.Entities;

public class Invoice
{
    public Guid Id { get; private set; }
    public string InvoiceNumber { get; private set; } = default!;
    public decimal TotalAmount { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Invoice() { } // EF Core

    public static Invoice Create(string invoiceNumber, decimal totalAmount, DateTime issuedAt, DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number is required", nameof(invoiceNumber));
        if (totalAmount < 0)
            throw new ArgumentException("Total amount cannot be negative", nameof(totalAmount));
        if (dueDate < issuedAt)
            throw new ArgumentException("Due date cannot be before issued date", nameof(dueDate));

        return new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber.Trim().ToUpperInvariant(),
            TotalAmount = totalAmount,
            IssuedAt = issuedAt,
            DueDate = dueDate,
            Status = InvoiceStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(DateTime dueDate)
    {
        if (dueDate < IssuedAt)
            throw new ArgumentException("Due date cannot be before issued date", nameof(dueDate));
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(InvoiceStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
