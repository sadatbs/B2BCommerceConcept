namespace B2B.Commerce.Contracts.Requisitions;

public record RequisitionDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string Status { get; init; }
    public decimal TotalAmount { get; init; }
    public string? RejectionReason { get; init; }
    public required DateTime SubmittedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public int LineItemCount { get; init; }
}
