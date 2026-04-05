using B2B.Commerce.Api.Filters;
using B2B.Commerce.Api.Mapping;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Contracts.Pricing;
using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace B2B.Commerce.Api.Endpoints;

public static class PriceTierEndpoints
{
    public static void MapPriceTierEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/price-tiers")
            .WithTags("Pricing")
            .WithOpenApi();

        group.MapGet("/", GetPriceTiers)
            .WithName("GetPriceTiers")
            .WithSummary("Get all price tiers");

        group.MapGet("/{id:guid}", GetPriceTierById)
            .WithName("GetPriceTierById")
            .WithSummary("Get price tier with prices");

        group.MapPost("/", CreatePriceTier)
            .WithName("CreatePriceTier")
            .WithSummary("Create a new price tier")
            .AddEndpointFilter<ValidationFilter<CreatePriceTierRequest>>();

        group.MapPut("/{id:guid}", UpdatePriceTier)
            .WithName("UpdatePriceTier")
            .WithSummary("Update price tier details")
            .AddEndpointFilter<ValidationFilter<CreatePriceTierRequest>>();

        group.MapDelete("/{id:guid}", DeletePriceTier)
            .WithName("DeletePriceTier")
            .WithSummary("Delete a price tier");

        group.MapGet("/{id:guid}/prices", GetTierPrices)
            .WithName("GetTierPrices")
            .WithSummary("Get all prices for a tier");

        group.MapPut("/{id:guid}/prices", SetTierPrice)
            .WithName("SetTierPrice")
            .WithSummary("Set price for a product in this tier")
            .AddEndpointFilter<ValidationFilter<SetTierPriceRequest>>();

        group.MapDelete("/{id:guid}/prices/{productId:guid}", RemoveTierPrice)
            .WithName("RemoveTierPrice")
            .WithSummary("Remove price for a product from this tier");
    }

    private static async Task<Ok<IReadOnlyList<PriceTierDto>>> GetPriceTiers(
        IPriceTierRepository repository,
        CancellationToken cancellationToken)
    {
        var tiers = await repository.GetAllAsync(cancellationToken);
        return TypedResults.Ok(tiers.ToDtos());
    }

    private static async Task<Results<Ok<PriceTierDetailDto>, NotFound<ErrorResponse>>> GetPriceTierById(
        Guid id,
        IPriceTierRepository repository,
        CancellationToken cancellationToken)
    {
        var tier = await repository.GetByIdWithPricesAsync(id, cancellationToken);

        if (tier is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("PriceTier", id.ToString()));

        return TypedResults.Ok(tier.ToDetailDto());
    }

    private static async Task<Created<PriceTierDto>> CreatePriceTier(
        CreatePriceTierRequest request,
        IPriceTierRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var tier = PriceTier.Create(request.Name, request.Description);

        await repository.AddAsync(tier, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/price-tiers/{tier.Id}", tier.ToDto());
    }

    private static async Task<Results<Ok<PriceTierDto>, NotFound<ErrorResponse>>> UpdatePriceTier(
        Guid id,
        CreatePriceTierRequest request,
        IPriceTierRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var tier = await repository.GetByIdAsync(id, cancellationToken);

        if (tier is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("PriceTier", id.ToString()));

        tier.UpdateDetails(request.Name, request.Description);
        repository.Update(tier);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(tier.ToDto());
    }

    private static async Task<Results<NoContent, NotFound<ErrorResponse>>> DeletePriceTier(
        Guid id,
        IPriceTierRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (!await repository.ExistsAsync(id, cancellationToken))
            return TypedResults.NotFound(ErrorResponse.NotFound("PriceTier", id.ToString()));

        await repository.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<IReadOnlyList<TierPriceDto>>, NotFound<ErrorResponse>>> GetTierPrices(
        Guid id,
        IPriceTierRepository repository,
        CancellationToken cancellationToken)
    {
        if (!await repository.ExistsAsync(id, cancellationToken))
            return TypedResults.NotFound(ErrorResponse.NotFound("PriceTier", id.ToString()));

        var prices = await repository.GetTierPricesAsync(id, cancellationToken);
        return TypedResults.Ok(prices.Select(p => p.ToDto()).ToList() as IReadOnlyList<TierPriceDto>);
    }

    private static async Task<Results<Ok<TierPriceDto>, NotFound<ErrorResponse>>> SetTierPrice(
        Guid id,
        SetTierPriceRequest request,
        IPriceTierRepository tierRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (!await tierRepository.ExistsAsync(id, cancellationToken))
            return TypedResults.NotFound(ErrorResponse.NotFound("PriceTier", id.ToString()));

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Product", request.ProductId.ToString()));

        await tierRepository.SetTierPriceAsync(id, request.ProductId, request.Price, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tierPrice = await tierRepository.GetTierPriceAsync(id, request.ProductId, cancellationToken);
        return TypedResults.Ok(tierPrice!.ToDto());
    }

    private static async Task<Results<NoContent, NotFound<ErrorResponse>>> RemoveTierPrice(
        Guid id,
        Guid productId,
        IPriceTierRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (!await repository.ExistsAsync(id, cancellationToken))
            return TypedResults.NotFound(ErrorResponse.NotFound("PriceTier", id.ToString()));

        await repository.RemoveTierPriceAsync(id, productId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
