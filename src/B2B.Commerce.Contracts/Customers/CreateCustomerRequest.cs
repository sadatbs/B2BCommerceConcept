namespace B2B.Commerce.Contracts.Customers;

public record CreateCustomerRequest
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? PaymentTerms { get; init; }
    public Guid? PriceTierId { get; init; }
}
