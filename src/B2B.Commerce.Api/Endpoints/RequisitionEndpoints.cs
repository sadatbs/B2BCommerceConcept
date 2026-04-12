using B2B.Commerce.Api.Mapping;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Contracts.Requisitions;
using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Enums;
using B2B.Commerce.Domain.Events;
using B2B.Commerce.Domain.Interfaces;
using B2B.Commerce.Domain.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace B2B.Commerce.Api.Endpoints;

public static class RequisitionEndpoints
{
    public static void MapRequisitionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/requisitions")
            .WithTags("Requisitions")
            .WithOpenApi();

        group.MapPost("/", SubmitRequisition)
            .WithName("SubmitRequisition")
            .WithSummary("Submit cart as a requisition (Buyer/Requisitioner)");

        group.MapGet("/{id:guid}", GetRequisitionById)
            .WithName("GetRequisitionById")
            .WithSummary("Get requisition details");

        group.MapGet("/user/{userId:guid}", GetRequisitionsByUser)
            .WithName("GetRequisitionsByUser")
            .WithSummary("Get requisitions for a user");

        group.MapPatch("/{id:guid}/approve", ApproveRequisition)
            .WithName("ApproveRequisition")
            .WithSummary("Approve a requisition (Approver role only)");

        group.MapPatch("/{id:guid}/reject", RejectRequisition)
            .WithName("RejectRequisition")
            .WithSummary("Reject a requisition (Approver role only)");
    }

    private static async Task<Results<Created<RequisitionDto>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> SubmitRequisition(
        SubmitRequisitionRequest request,
        ICartRepository cartRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        IRequisitionRepository requisitionRepository,
        IOrderRepository orderRepository,
        IPricingService pricingService,
        IUnitOfWork unitOfWork,
        IEventDispatcher eventDispatcher,
        CancellationToken cancellationToken)
    {
        // 1. Load cart with items
        var cart = await cartRepository.GetByIdWithItemsAsync(request.CartId, cancellationToken);
        if (cart is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Cart", request.CartId.ToString()));

        if (cart.IsEmpty)
            return TypedResults.BadRequest(new ErrorResponse
            {
                Code = "EMPTY_CART",
                Message = "Cannot submit an empty cart as a requisition"
            });

        // 2. Verify cart belongs to user
        if (cart.UserId != request.UserId)
            return TypedResults.BadRequest(new ErrorResponse
            {
                Code = "CART_OWNERSHIP_MISMATCH",
                Message = "Cart does not belong to the specified user"
            });

        // 3. Load user for budget check
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("User", request.UserId.ToString()));

        // 4. Build product snapshot using IPricingService (customer-specific prices)
        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var prices = await pricingService.GetPricesAsync(productIds, cart.CustomerId, cancellationToken);

        var productSnapshot = new Dictionary<Guid, (string Sku, string Name, decimal Price)>();
        foreach (var cartItem in cart.Items)
        {
            var product = await productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken);
            if (product is null)
                return TypedResults.NotFound(ErrorResponse.NotFound("Product", cartItem.ProductId.ToString()));

            var resolvedPrice = prices.GetValueOrDefault(cartItem.ProductId, product.Price);
            productSnapshot[cartItem.ProductId] = (product.Sku, product.Name, resolvedPrice);
        }

        // 5. Create Requisition (prices frozen at this point)
        var requisition = Requisition.CreateFromCart(cart, productSnapshot);

        // 6. Budget check → auto-approve if within limit
        var requiresApproval = user.BudgetLimit.HasValue && requisition.TotalAmount > user.BudgetLimit.Value;

        if (!requiresApproval)
        {
            // Auto-approve: create Order immediately
            requisition.Approve();
            var order = Order.CreateFromRequisition(requisition);
            requisition.MarkOrdered();

            await requisitionRepository.AddAsync(requisition, cancellationToken);
            await orderRepository.AddAsync(order, cancellationToken);

            cart.Clear();
            cartRepository.Update(cart);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
            order.ClearDomainEvents();
        }
        else
        {
            // Requires approval: save Requisition in Submitted state, clear cart
            await requisitionRepository.AddAsync(requisition, cancellationToken);

            cart.Clear();
            cartRepository.Update(cart);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return TypedResults.Created($"/api/requisitions/{requisition.Id}", requisition.ToDto());
    }

    private static async Task<Results<Ok<RequisitionDetailDto>, NotFound<ErrorResponse>>> GetRequisitionById(
        Guid id,
        IRequisitionRepository repository,
        CancellationToken cancellationToken)
    {
        var requisition = await repository.GetByIdWithLineItemsAsync(id, cancellationToken);
        if (requisition is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Requisition", id.ToString()));

        return TypedResults.Ok(requisition.ToDetailDto());
    }

    private static async Task<Ok<PagedResponse<RequisitionDto>>> GetRequisitionsByUser(
        Guid userId,
        [AsParameters] PagedRequest request,
        IRequisitionRepository repository,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetByUserIdPagedAsync(
            userId, request.Skip, request.PageSize, cancellationToken);

        return TypedResults.Ok(new PagedResponse<RequisitionDto>
        {
            Items = items.Select(r => r.ToDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    private static async Task<Results<Ok<RequisitionDto>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>, StatusCodeHttpResult>> ApproveRequisition(
        Guid id,
        string? requestingUserRole,
        IRequisitionRepository requisitionRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IEventDispatcher eventDispatcher,
        CancellationToken cancellationToken)
    {
        // Role gate: only Approver can approve
        if (!Enum.TryParse<UserRole>(requestingUserRole, true, out var role) || role != UserRole.Approver)
            return TypedResults.StatusCode(StatusCodes.Status403Forbidden);

        var requisition = await requisitionRepository.GetByIdWithLineItemsAsync(id, cancellationToken);
        if (requisition is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Requisition", id.ToString()));

        try
        {
            requisition.Approve();
            var order = Order.CreateFromRequisition(requisition);
            requisition.MarkOrdered();

            await orderRepository.AddAsync(order, cancellationToken);
            requisitionRepository.Update(requisition);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
            order.ClearDomainEvents();

            return TypedResults.Ok(requisition.ToDto());
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

    private static async Task<Results<Ok<RequisitionDto>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>, StatusCodeHttpResult>> RejectRequisition(
        Guid id,
        RejectRequisitionRequest request,
        string? requestingUserRole,
        IRequisitionRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        // Role gate: only Approver can reject
        if (!Enum.TryParse<UserRole>(requestingUserRole, true, out var role) || role != UserRole.Approver)
            return TypedResults.StatusCode(StatusCodes.Status403Forbidden);

        var requisition = await repository.GetByIdAsync(id, cancellationToken);
        if (requisition is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Requisition", id.ToString()));

        try
        {
            requisition.Reject(request.Reason);
            repository.Update(requisition);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return TypedResults.Ok(requisition.ToDto());
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
