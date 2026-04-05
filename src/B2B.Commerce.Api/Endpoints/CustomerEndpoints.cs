using B2B.Commerce.Api.Filters;
using B2B.Commerce.Api.Mapping;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Contracts.Customers;
using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Enums;
using B2B.Commerce.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace B2B.Commerce.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers")
            .WithTags("Customers")
            .WithOpenApi();

        group.MapGet("/", GetCustomers)
            .WithName("GetCustomers")
            .WithSummary("Get all customers with pagination");

        group.MapGet("/{id:guid}", GetCustomerById)
            .WithName("GetCustomerById")
            .WithSummary("Get customer with users");

        group.MapGet("/code/{code}", GetCustomerByCode)
            .WithName("GetCustomerByCode")
            .WithSummary("Get customer by code");

        group.MapPost("/", CreateCustomer)
            .WithName("CreateCustomer")
            .WithSummary("Create a new customer")
            .AddEndpointFilter<ValidationFilter<CreateCustomerRequest>>();

        group.MapPut("/{id:guid}", UpdateCustomer)
            .WithName("UpdateCustomer")
            .WithSummary("Update customer details")
            .AddEndpointFilter<ValidationFilter<UpdateCustomerRequest>>();

        group.MapPut("/{id:guid}/price-tier", AssignPriceTier)
            .WithName("AssignPriceTier")
            .WithSummary("Assign price tier to customer");

        group.MapPost("/{id:guid}/activate", ActivateCustomer)
            .WithName("ActivateCustomer")
            .WithSummary("Activate a customer");

        group.MapPost("/{id:guid}/deactivate", DeactivateCustomer)
            .WithName("DeactivateCustomer")
            .WithSummary("Deactivate a customer");

        group.MapGet("/{id:guid}/users", GetCustomerUsers)
            .WithName("GetCustomerUsers")
            .WithSummary("Get users for a customer");

        group.MapPost("/{id:guid}/users", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a user for a customer")
            .AddEndpointFilter<ValidationFilter<CreateUserRequest>>();
    }

    private static async Task<Ok<PagedResponse<CustomerDto>>> GetCustomers(
        [AsParameters] PagedRequest request,
        ICustomerRepository repository,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPagedAsync(request.Skip, request.PageSize, cancellationToken);

        return TypedResults.Ok(new PagedResponse<CustomerDto>
        {
            Items = items.ToDtos(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    private static async Task<Results<Ok<CustomerDetailDto>, NotFound<ErrorResponse>>> GetCustomerById(
        Guid id,
        ICustomerRepository repository,
        CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdWithUsersAsync(id, cancellationToken);

        if (customer is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Customer", id.ToString()));

        return TypedResults.Ok(customer.ToDetailDto());
    }

    private static async Task<Results<Ok<CustomerDto>, NotFound<ErrorResponse>>> GetCustomerByCode(
        string code,
        ICustomerRepository repository,
        CancellationToken cancellationToken)
    {
        var customer = await repository.GetByCodeAsync(code, cancellationToken);

        if (customer is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Customer", code));

        return TypedResults.Ok(customer.ToDto());
    }

    private static async Task<Results<Created<CustomerDto>, Conflict<ErrorResponse>>> CreateCustomer(
        CreateCustomerRequest request,
        ICustomerRepository repository,
        IPriceTierRepository tierRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (await repository.CodeExistsAsync(request.Code, cancellationToken))
        {
            return TypedResults.Conflict(new ErrorResponse
            {
                Code = "DUPLICATE_CODE",
                Message = $"Customer with code '{request.Code}' already exists"
            });
        }

        if (request.PriceTierId.HasValue && !await tierRepository.ExistsAsync(request.PriceTierId.Value, cancellationToken))
        {
            return TypedResults.Conflict(new ErrorResponse
            {
                Code = "INVALID_PRICE_TIER",
                Message = "Price tier not found"
            });
        }

        var paymentTerms = Enum.TryParse<PaymentTerms>(request.PaymentTerms, true, out var terms)
            ? terms
            : PaymentTerms.Net30;

        var customer = Customer.Create(request.Code, request.Name, request.Email, paymentTerms, request.PriceTierId);

        await repository.AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/customers/{customer.Id}", customer.ToDto());
    }

    private static async Task<Results<Ok<CustomerDto>, NotFound<ErrorResponse>>> UpdateCustomer(
        Guid id,
        UpdateCustomerRequest request,
        ICustomerRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Customer", id.ToString()));

        var paymentTerms = Enum.TryParse<PaymentTerms>(request.PaymentTerms, true, out var terms)
            ? terms
            : customer.PaymentTerms;

        customer.UpdateDetails(request.Name, request.Email, paymentTerms);
        repository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(customer.ToDto());
    }

    private static async Task<Results<Ok<CustomerDto>, NotFound<ErrorResponse>>> AssignPriceTier(
        Guid id,
        AssignPriceTierRequest request,
        ICustomerRepository repository,
        IPriceTierRepository tierRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Customer", id.ToString()));

        if (request.PriceTierId.HasValue && !await tierRepository.ExistsAsync(request.PriceTierId.Value, cancellationToken))
            return TypedResults.NotFound(ErrorResponse.NotFound("PriceTier", request.PriceTierId.Value.ToString()));

        customer.AssignPriceTier(request.PriceTierId);
        repository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-fetch to get price tier name
        customer = await repository.GetByIdAsync(id, cancellationToken);
        return TypedResults.Ok(customer!.ToDto());
    }

    private static async Task<Results<Ok<CustomerDto>, NotFound<ErrorResponse>>> ActivateCustomer(
        Guid id,
        ICustomerRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Customer", id.ToString()));

        customer.Activate();
        repository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(customer.ToDto());
    }

    private static async Task<Results<Ok<CustomerDto>, NotFound<ErrorResponse>>> DeactivateCustomer(
        Guid id,
        ICustomerRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Customer", id.ToString()));

        customer.Deactivate();
        repository.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(customer.ToDto());
    }

    private static async Task<Results<Ok<PagedResponse<UserDto>>, NotFound<ErrorResponse>>> GetCustomerUsers(
        Guid id,
        ICustomerRepository customerRepository,
        IUserRepository userRepository,
        CancellationToken cancellationToken,
        int? page = null,
        int? pageSize = null)
    {
        var p = page ?? 1;
        var ps = pageSize ?? 20;

        if (!await customerRepository.ExistsAsync(id, cancellationToken))
            return TypedResults.NotFound(ErrorResponse.NotFound("Customer", id.ToString()));

        var skip = (p - 1) * ps;
        var (items, totalCount) = await userRepository.GetByCustomerIdPagedAsync(id, skip, ps, cancellationToken);

        return TypedResults.Ok(new PagedResponse<UserDto>
        {
            Items = items.ToDtos(),
            Page = p,
            PageSize = ps,
            TotalCount = totalCount
        });
    }

    private static async Task<Results<Created<UserDto>, NotFound<ErrorResponse>, Conflict<ErrorResponse>>> CreateUser(
        Guid id,
        CreateUserRequest request,
        ICustomerRepository customerRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        if (!await customerRepository.ExistsAsync(id, cancellationToken))
            return TypedResults.NotFound(ErrorResponse.NotFound("Customer", id.ToString()));

        if (await userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return TypedResults.Conflict(new ErrorResponse
            {
                Code = "DUPLICATE_EMAIL",
                Message = $"User with email '{request.Email}' already exists"
            });
        }

        var role = Enum.TryParse<UserRole>(request.Role, true, out var parsedRole)
            ? parsedRole
            : UserRole.Buyer;

        var user = User.Create(id, request.Email, request.FirstName, request.LastName, role);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/users/{user.Id}", user.ToDto());
    }
}
