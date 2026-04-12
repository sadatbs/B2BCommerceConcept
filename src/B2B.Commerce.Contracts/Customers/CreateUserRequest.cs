namespace B2B.Commerce.Contracts.Customers;

public record CreateUserRequest
{
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Role { get; init; }
    public decimal? BudgetLimit { get; init; }
}
