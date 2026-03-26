namespace B2B.Commerce.Contracts.Products;

public record UpdateProductPriceRequest
{
    public required decimal Price { get; init; }
}
