using B2B.Commerce.Api.Filters;
using B2B.Commerce.Api.Mapping;
using B2B.Commerce.Contracts.Carts;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace B2B.Commerce.Api.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/carts")
            .WithTags("Carts")
            .WithOpenApi();

        group.MapPost("/", CreateCart)
            .WithName("CreateCart")
            .WithSummary("Create a new cart");

        group.MapGet("/{id:guid}", GetCart)
            .WithName("GetCart")
            .WithSummary("Get cart with items");

        group.MapPost("/{id:guid}/items", AddToCart)
            .WithName("AddToCart")
            .WithSummary("Add item to cart")
            .AddEndpointFilter<ValidationFilter<AddToCartRequest>>();

        group.MapPut("/{id:guid}/items/{productId:guid}", UpdateCartItem)
            .WithName("UpdateCartItem")
            .WithSummary("Update item quantity")
            .AddEndpointFilter<ValidationFilter<UpdateCartItemRequest>>();

        group.MapDelete("/{id:guid}/items/{productId:guid}", RemoveFromCart)
            .WithName("RemoveFromCart")
            .WithSummary("Remove item from cart");

        group.MapDelete("/{id:guid}", ClearCart)
            .WithName("ClearCart")
            .WithSummary("Clear all items from cart");
    }

    private static async Task<Created<CartDto>> CreateCart(
        CreateCartRequest? request,
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var cart = Cart.Create(request?.CustomerId);
        await cartRepository.AddAsync(cart, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/carts/{cart.Id}", cart.ToDto());
    }

    private static async Task<Results<Ok<CartDto>, NotFound<ErrorResponse>>> GetCart(
        Guid id,
        ICartRepository cartRepository,
        CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByIdWithItemsAsync(id, cancellationToken);

        if (cart is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Cart", id.ToString()));

        return TypedResults.Ok(cart.ToDto());
    }

    private static async Task<Results<Ok<CartDto>, NotFound<ErrorResponse>>> AddToCart(
        Guid id,
        AddToCartRequest request,
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByIdWithItemsAsync(id, cancellationToken);

        if (cart is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Cart", id.ToString()));

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Product", request.ProductId.ToString()));

        cart.AddItem(request.ProductId, request.Quantity);
        cartRepository.Update(cart);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        cart = await cartRepository.GetByIdWithItemsAsync(id, cancellationToken);
        return TypedResults.Ok(cart!.ToDto());
    }

    private static async Task<Results<Ok<CartDto>, NotFound<ErrorResponse>>> UpdateCartItem(
        Guid id,
        Guid productId,
        UpdateCartItemRequest request,
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByIdWithItemsAsync(id, cancellationToken);

        if (cart is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Cart", id.ToString()));

        try
        {
            cart.UpdateItemQuantity(productId, request.Quantity);
            cartRepository.Update(cart);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            cart = await cartRepository.GetByIdWithItemsAsync(id, cancellationToken);
            return TypedResults.Ok(cart!.ToDto());
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound(ErrorResponse.NotFound("Product in cart", productId.ToString()));
        }
    }

    private static async Task<Results<Ok<CartDto>, NotFound<ErrorResponse>>> RemoveFromCart(
        Guid id,
        Guid productId,
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByIdWithItemsAsync(id, cancellationToken);

        if (cart is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Cart", id.ToString()));

        try
        {
            cart.RemoveItem(productId);
            cartRepository.Update(cart);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            cart = await cartRepository.GetByIdWithItemsAsync(id, cancellationToken);
            return TypedResults.Ok(cart!.ToDto());
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound(ErrorResponse.NotFound("Product in cart", productId.ToString()));
        }
    }

    private static async Task<Results<Ok<CartDto>, NotFound<ErrorResponse>>> ClearCart(
        Guid id,
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByIdWithItemsAsync(id, cancellationToken);

        if (cart is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Cart", id.ToString()));

        cart.Clear();
        cartRepository.Update(cart);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(cart.ToDto());
    }
}
