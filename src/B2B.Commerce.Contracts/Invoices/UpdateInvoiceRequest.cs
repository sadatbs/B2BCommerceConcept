namespace B2B.Commerce.Contracts.Invoices;

public record UpdateInvoiceRequest
{
    public required DateTime DueDate { get; init; }
    public required string Status { get; init; }
}
