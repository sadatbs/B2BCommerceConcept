using B2B.Commerce.Api.Mapping;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Contracts.Products;
using B2B.Commerce.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace B2B.Commerce.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .WithOpenApi();

        group.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Get all products with pagination");

        group.MapGet("/{id:guid}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("Get a product by ID");

        group.MapGet("/sku/{sku}", GetProductBySku)
            .WithName("GetProductBySku")
            .WithSummary("Get a product by SKU");

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create a new product");

        group.MapPut("/{id:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("Update product details");

        group.MapPatch("/{id:guid}/price", UpdateProductPrice)
            .WithName("UpdateProductPrice")
            .WithSummary("Update product price");

        group.MapDelete("/{id:guid}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product");
    }

    private static async Task<Ok<PagedResponse<ProductDto>>> GetProducts(
        [AsParameters] PagedRequest request,
        IProductRepository repository,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPagedAsync(
            request.Skip,
            request.PageSize,
            cancellationToken);

        var response = new PagedResponse<ProductDto>
        {
            Items = items.ToDtos(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<ProductDto>, NotFound<ErrorResponse>>> GetProductById(
        Guid id,
        IProductRepository repository,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);

        if (product is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Product", id.ToString()));

        return TypedResults.Ok(product.ToDto());
    }

    private static async Task<Results<Ok<ProductDto>, NotFound<ErrorResponse>>> GetProductBySku(
        string sku,
        IProductRepository repository,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetBySkuAsync(sku, cancellationToken);

        if (product is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Product", sku));

        return TypedResults.Ok(product.ToDto());
    }

    private static async Task<Results<Created<ProductDto>, Conflict<ErrorResponse>>> CreateProduct(
        CreateProductRequest request,
        IProductRepository repository,
        CancellationToken cancellationToken)
    {
        if (await repository.SkuExistsAsync(request.Sku, cancellationToken))
        {
            return TypedResults.Conflict(
                ErrorResponse.Conflict($"A product with SKU '{request.Sku}' already exists."));
        }

        var product = request.ToEntity();
        await repository.AddAsync(product, cancellationToken);

        return TypedResults.Created($"/api/products/{product.Id}", product.ToDto());
    }

    private static async Task<Results<Ok<ProductDto>, NotFound<ErrorResponse>>> UpdateProduct(
        Guid id,
        UpdateProductRequest request,
        IProductRepository repository,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);

        if (product is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Product", id.ToString()));

        product.UpdateDetails(request.Name, request.Description, request.Category);
        await repository.UpdateAsync(product, cancellationToken);

        return TypedResults.Ok(product.ToDto());
    }

    private static async Task<Results<Ok<ProductDto>, NotFound<ErrorResponse>>> UpdateProductPrice(
        Guid id,
        UpdateProductPriceRequest request,
        IProductRepository repository,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);

        if (product is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Product", id.ToString()));

        product.UpdatePrice(request.Price);
        await repository.UpdateAsync(product, cancellationToken);

        return TypedResults.Ok(product.ToDto());
    }

    private static async Task<Results<NoContent, NotFound<ErrorResponse>>> DeleteProduct(
        Guid id,
        IProductRepository repository,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);

        if (product is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Product", id.ToString()));

        await repository.DeleteAsync(id, cancellationToken);

        return TypedResults.NoContent();
    }
}
