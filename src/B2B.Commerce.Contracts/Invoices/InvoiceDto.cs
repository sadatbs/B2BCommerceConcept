namespace B2B.Commerce.Contracts.Invoices;

public record InvoiceDto
{
    public required Guid Id { get; init; }
    public required string InvoiceNumber { get; init; }
    public required decimal TotalAmount { get; init; }
    public required DateTime IssuedAt { get; init; }
    public required DateTime DueDate { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
