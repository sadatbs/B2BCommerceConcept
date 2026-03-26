using B2B.Commerce.Api.Filters;
using B2B.Commerce.Api.Mapping;
using B2B.Commerce.Contracts.Catalogs;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Contracts.Products;
using B2B.Commerce.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace B2B.Commerce.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/catalogs")
            .WithTags("Catalogs")
            .WithOpenApi();

        group.MapGet("/", GetCatalogs)
            .WithName("GetCatalogs")
            .WithSummary("Get all catalogs with pagination");

        group.MapGet("/{id:guid}", GetCatalogById)
            .WithName("GetCatalogById")
            .WithSummary("Get a catalog by ID (includes products)");

        group.MapPost("/", CreateCatalog)
            .WithName("CreateCatalog")
            .WithSummary("Create a new catalog")
            .AddEndpointFilter<ValidationFilter<CreateCatalogRequest>>();

        group.MapPut("/{id:guid}", UpdateCatalog)
            .WithName("UpdateCatalog")
            .WithSummary("Update catalog details")
            .AddEndpointFilter<ValidationFilter<UpdateCatalogRequest>>();

        group.MapDelete("/{id:guid}", DeleteCatalog)
            .WithName("DeleteCatalog")
            .WithSummary("Delete a catalog");

        group.MapGet("/{id:guid}/products", GetCatalogProducts)
            .WithName("GetCatalogProducts")
            .WithSummary("Get products in a catalog (paginated)");

        group.MapPost("/{id:guid}/products", AddProductsToCatalog)
            .WithName("AddProductsToCatalog")
            .WithSummary("Add products to a catalog")
            .AddEndpointFilter<ValidationFilter<AddProductsToCatalogRequest>>();

        group.MapDelete("/{id:guid}/products/{productId:guid}", RemoveProductFromCatalog)
            .WithName("RemoveProductFromCatalog")
            .WithSummary("Remove a product from a catalog");
    }

    private static async Task<Ok<PagedResponse<CatalogDto>>> GetCatalogs(
        [AsParameters] PagedRequest request,
        ICatalogRepository repository,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPagedAsync(
            request.Skip, request.PageSize, cancellationToken);

        var response = new PagedResponse<CatalogDto>
        {
            Items = items.ToDtos(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<CatalogDetailDto>, NotFound<ErrorResponse>>> GetCatalogById(
        Guid id,
        ICatalogRepository repository,
        CancellationToken cancellationToken)
    {
        var catalog = await repository.GetByIdWithProductsAsync(id, cancellationToken);

        if (catalog is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Catalog", id.ToString()));

        return TypedResults.Ok(catalog.ToDetailDto());
    }

    private static async Task<Created<CatalogDto>> CreateCatalog(
        CreateCatalogRequest request,
        ICatalogRepository repository,
        CancellationToken cancellationToken)
    {
        var catalog = request.ToEntity();
        await repository.AddAsync(catalog, cancellationToken);

        return TypedResults.Created($"/api/catalogs/{catalog.Id}", catalog.ToDto(0));
    }

    private static async Task<Results<Ok<CatalogDto>, NotFound<ErrorResponse>>> UpdateCatalog(
        Guid id,
        UpdateCatalogRequest request,
        ICatalogRepository repository,
        CancellationToken cancellationToken)
    {
        var catalog = await repository.GetByIdAsync(id, cancellationToken);

        if (catalog is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Catalog", id.ToString()));

        catalog.UpdateDetails(request.Name, request.Description);
        await repository.UpdateAsync(catalog, cancellationToken);

        return TypedResults.Ok(catalog.ToDto(0));
    }

    private static async Task<Results<NoContent, NotFound<ErrorResponse>>> DeleteCatalog(
        Guid id,
        ICatalogRepository repository,
        CancellationToken cancellationToken)
    {
        var exists = await repository.ExistsAsync(id, cancellationToken);

        if (!exists)
            return TypedResults.NotFound(ErrorResponse.NotFound("Catalog", id.ToString()));

        await repository.DeleteAsync(id, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<PagedResponse<ProductDto>>, NotFound<ErrorResponse>>> GetCatalogProducts(
        Guid id,
        [AsParameters] PagedRequest request,
        ICatalogRepository catalogRepository,
        CancellationToken cancellationToken)
    {
        var exists = await catalogRepository.ExistsAsync(id, cancellationToken);

        if (!exists)
            return TypedResults.NotFound(ErrorResponse.NotFound("Catalog", id.ToString()));

        var (items, totalCount) = await catalogRepository.GetProductsInCatalogPagedAsync(
            id, request.Skip, request.PageSize, cancellationToken);

        var response = new PagedResponse<ProductDto>
        {
            Items = items.ToDtos(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<int>, NotFound<ErrorResponse>, Conflict<ErrorResponse>>> AddProductsToCatalog(
        Guid id,
        AddProductsToCatalogRequest request,
        ICatalogRepository catalogRepository,
        IProductRepository productRepository,
        CancellationToken cancellationToken)
    {
        var catalogExists = await catalogRepository.ExistsAsync(id, cancellationToken);
        if (!catalogExists)
            return TypedResults.NotFound(ErrorResponse.NotFound("Catalog", id.ToString()));

        var productsToAdd = new List<Guid>();
        var notFoundProducts = new List<string>();

        foreach (var productId in request.ProductIds.Distinct())
        {
            var product = await productRepository.GetByIdAsync(productId, cancellationToken);

            if (product is null)
            {
                notFoundProducts.Add(productId.ToString());
                continue;
            }

            var alreadyAdded = await catalogRepository.IsProductInCatalogAsync(id, productId, cancellationToken);
            if (!alreadyAdded)
                productsToAdd.Add(productId);
        }

        if (notFoundProducts.Count > 0)
            return TypedResults.NotFound(ErrorResponse.NotFound("Products", string.Join(", ", notFoundProducts)));

        if (productsToAdd.Count > 0)
            await catalogRepository.AddProductsToCatalogAsync(id, productsToAdd, cancellationToken);

        return TypedResults.Ok(productsToAdd.Count);
    }

    private static async Task<Results<NoContent, NotFound<ErrorResponse>>> RemoveProductFromCatalog(
        Guid id,
        Guid productId,
        ICatalogRepository repository,
        CancellationToken cancellationToken)
    {
        var catalogExists = await repository.ExistsAsync(id, cancellationToken);
        if (!catalogExists)
            return TypedResults.NotFound(ErrorResponse.NotFound("Catalog", id.ToString()));

        var isInCatalog = await repository.IsProductInCatalogAsync(id, productId, cancellationToken);
        if (!isInCatalog)
            return TypedResults.NotFound(ErrorResponse.NotFound("Product in catalog", productId.ToString()));

        await repository.RemoveProductFromCatalogAsync(id, productId, cancellationToken);

        return TypedResults.NoContent();
    }
}
