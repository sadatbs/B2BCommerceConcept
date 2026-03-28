using B2B.Commerce.Api.Filters;
using B2B.Commerce.Api.Mapping;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Contracts.Orders;
using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Events;
using B2B.Commerce.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace B2B.Commerce.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        group.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .WithSummary("Create order from cart (checkout)")
            .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>();

        group.MapGet("/", GetOrders)
            .WithName("GetOrders")
            .WithSummary("Get all orders with pagination");

        group.MapGet("/{id:guid}", GetOrderById)
            .WithName("GetOrderById")
            .WithSummary("Get order details");

        group.MapPost("/{id:guid}/confirm", ConfirmOrder)
            .WithName("ConfirmOrder")
            .WithSummary("Confirm a pending order");

        group.MapPost("/{id:guid}/complete", CompleteOrder)
            .WithName("CompleteOrder")
            .WithSummary("Mark order as completed");
    }

    private static async Task<Results<Created<OrderDto>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> CreateOrder(
        CreateOrderRequest request,
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IEventDispatcher eventDispatcher,
        CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByIdWithItemsAsync(request.CartId, cancellationToken);

        if (cart is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Cart", request.CartId.ToString()));

        if (cart.IsEmpty)
            return TypedResults.BadRequest(new ErrorResponse
            {
                Code = "EMPTY_CART",
                Message = "Cannot create order from empty cart"
            });

        // Fetch all products referenced in the cart
        var products = new Dictionary<Guid, Product>();
        foreach (var productId in cart.Items.Select(i => i.ProductId).Distinct())
        {
            var product = await productRepository.GetByIdAsync(productId, cancellationToken);
            if (product is null)
                return TypedResults.NotFound(ErrorResponse.NotFound("Product", productId.ToString()));

            products[productId] = product;
        }

        var order = Order.CreateFromCart(
            cart,
            id => products.GetValueOrDefault(id),
            request.PurchaseOrderNumber);

        cart.Clear();

        await orderRepository.AddAsync(order, cancellationToken);
        cartRepository.Update(cart);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();

        return TypedResults.Created($"/api/orders/{order.Id}", order.ToDto());
    }

    private static async Task<Ok<PagedResponse<OrderDto>>> GetOrders(
        [AsParameters] PagedRequest request,
        IOrderRepository orderRepository,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await orderRepository.GetPagedAsync(
            request.Skip, request.PageSize, cancellationToken);

        var response = new PagedResponse<OrderDto>
        {
            Items = items.ToDtos(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<OrderDetailDto>, NotFound<ErrorResponse>>> GetOrderById(
        Guid id,
        IOrderRepository orderRepository,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(id, cancellationToken);

        if (order is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Order", id.ToString()));

        return TypedResults.Ok(order.ToDetailDto());
    }

    private static async Task<Results<Ok<OrderDto>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> ConfirmOrder(
        Guid id,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Order", id.ToString()));

        try
        {
            order.Confirm();
            orderRepository.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.Ok(order.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new ErrorResponse
            {
                Code = "INVALID_STATUS_TRANSITION",
                Message = ex.Message
            });
        }
    }

    private static async Task<Results<Ok<OrderDto>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> CompleteOrder(
        Guid id,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Order", id.ToString()));

        try
        {
            order.Complete();
            orderRepository.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.Ok(order.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new ErrorResponse
            {
                Code = "INVALID_STATUS_TRANSITION",
                Message = ex.Message
            });
        }
    }
}
