namespace B2B.Commerce.Contracts.Customers;

public record UpdateUserRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Role { get; init; }
}
