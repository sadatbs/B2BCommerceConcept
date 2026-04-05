namespace B2B.Commerce.Contracts.Customers;

public record UserDto
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string FullName { get; init; }
    public required string Role { get; init; }
    public bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
}
