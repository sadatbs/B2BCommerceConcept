namespace B2B.Commerce.Contracts.Carts;

public record CartDto
{
    public required Guid Id { get; init; }
    public Guid? CustomerId { get; init; }
    public required IReadOnlyList<CartItemDto> Items { get; init; }
    public decimal Subtotal { get; init; }
    public int ItemCount { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
