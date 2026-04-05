namespace B2B.Commerce.Contracts.Customers;

public record UpdateCustomerRequest
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string PaymentTerms { get; init; }
}
