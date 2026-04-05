namespace B2B.Commerce.Contracts.Invoices;

public record CreateInvoiceRequest
{
    public required string InvoiceNumber { get; init; }
    public required decimal TotalAmount { get; init; }
    public required DateTime IssuedAt { get; init; }
    public required DateTime DueDate { get; init; }
}
