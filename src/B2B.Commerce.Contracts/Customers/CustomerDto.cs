namespace B2B.Commerce.Contracts.Customers;

public record CustomerDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string PaymentTerms { get; init; }
    public Guid? PriceTierId { get; init; }
    public string? PriceTierName { get; init; }
    public bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
}
