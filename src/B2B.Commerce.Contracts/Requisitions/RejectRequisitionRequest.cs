namespace B2B.Commerce.Contracts.Requisitions;

public record RejectRequisitionRequest
{
    public required string Reason { get; init; }
}
