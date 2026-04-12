namespace B2B.Commerce.Contracts.Requisitions;

public record SubmitRequisitionRequest
{
    public required Guid CartId { get; init; }
    public required Guid UserId { get; init; }  // TODO: replace with auth claim in future
}
