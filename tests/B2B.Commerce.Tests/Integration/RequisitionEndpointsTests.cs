using System.Net;
using System.Net.Http.Json;
using B2B.Commerce.Contracts.Carts;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Contracts.Customers;
using B2B.Commerce.Contracts.Orders;
using B2B.Commerce.Contracts.Products;
using B2B.Commerce.Contracts.Requisitions;
using FluentAssertions;

namespace B2B.Commerce.Tests.Integration;

public class RequisitionEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task SubmitRequisition_UserWithNoBudgetLimit_AutoApproves_ReturnsOrdered()
    {
        var (user, customer, cart) = await CreateUserCartWithItemAsync(budgetLimit: null, productPrice: 100m);

        var response = await Client.PostAsJsonAsync("/api/requisitions",
            new SubmitRequisitionRequest { CartId = cart.Id, UserId = user.Id });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var requisition = await response.Content.ReadFromJsonAsync<RequisitionDto>();
        requisition!.Status.Should().Be("Ordered");
        requisition.TotalAmount.Should().Be(100m);
    }

    [Fact]
    public async Task SubmitRequisition_TotalExceedsBudget_RequiresApproval()
    {
        // Budget = $1, product = $100 → total ($100) > budget ($1) → requires approval
        var (user, customer, cart) = await CreateUserCartWithItemAsync(budgetLimit: 1m, productPrice: 100m);

        var response = await Client.PostAsJsonAsync("/api/requisitions",
            new SubmitRequisitionRequest { CartId = cart.Id, UserId = user.Id });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var requisition = await response.Content.ReadFromJsonAsync<RequisitionDto>();
        requisition!.Status.Should().Be("Submitted");
    }

    [Fact]
    public async Task SubmitRequisition_EmptyCart_ReturnsBadRequest()
    {
        var customer = await CreateCustomerAsync($"REQ-EC-{Uid()}", $"EC Customer", $"ec-{Uid()}@test.com");
        var user = await CreateUserAsync(customer.Id, $"ec-buyer-{Uid()}@test.com", "Buyer", "EC");
        var cartResponse = await Client.PostAsJsonAsync("/api/carts",
            new CreateCartRequest { UserId = user.Id, CustomerId = customer.Id });
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();

        var response = await Client.PostAsJsonAsync("/api/requisitions",
            new SubmitRequisitionRequest { CartId = cart!.Id, UserId = user.Id });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitRequisition_CartOwnershipMismatch_ReturnsBadRequest()
    {
        var customer = await CreateCustomerAsync($"REQ-OM-{Uid()}", "OM Customer", $"om-{Uid()}@test.com");
        var cartOwner = await CreateUserAsync(customer.Id, $"owner-{Uid()}@test.com", "Owner", "User");
        var otherUser = await CreateUserAsync(customer.Id, $"other-{Uid()}@test.com", "Other", "User");
        var product = await CreateProductAsync($"OM-{Uid()}", "OM Product", 50m);

        var cartResponse = await Client.PostAsJsonAsync("/api/carts",
            new CreateCartRequest { UserId = cartOwner.Id, CustomerId = customer.Id });
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();
        await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 1 });

        // Submit as a different user
        var response = await Client.PostAsJsonAsync("/api/requisitions",
            new SubmitRequisitionRequest { CartId = cart.Id, UserId = otherUser.Id });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRequisitionById_ReturnsDetailWithLineItems()
    {
        var (user, customer, cart) = await CreateUserCartWithItemAsync(budgetLimit: 1m, productPrice: 50m);
        var submitResponse = await Client.PostAsJsonAsync("/api/requisitions",
            new SubmitRequisitionRequest { CartId = cart.Id, UserId = user.Id });
        var submitted = await submitResponse.Content.ReadFromJsonAsync<RequisitionDto>();

        var response = await Client.GetAsync($"/api/requisitions/{submitted!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<RequisitionDetailDto>();
        detail!.LineItems.Should().HaveCount(1);
        detail.LineItems[0].UnitPrice.Should().Be(50m);
        detail.LineItems[0].Quantity.Should().Be(1);
    }

    [Fact]
    public async Task ApproveRequisition_ByApprover_CreatesOrder()
    {
        var (user, customer, cart) = await CreateUserCartWithItemAsync(budgetLimit: 1m, productPrice: 50m);
        var submitResponse = await Client.PostAsJsonAsync("/api/requisitions",
            new SubmitRequisitionRequest { CartId = cart.Id, UserId = user.Id });
        var submitted = await submitResponse.Content.ReadFromJsonAsync<RequisitionDto>();

        var response = await Client.PatchAsync(
            $"/api/requisitions/{submitted!.Id}/approve?requestingUserRole=Approver", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var approved = await response.Content.ReadFromJsonAsync<RequisitionDto>();
        approved!.Status.Should().Be("Ordered");
    }

    [Fact]
    public async Task ApproveRequisition_ByNonApprover_ReturnsForbidden()
    {
        var (user, customer, cart) = await CreateUserCartWithItemAsync(budgetLimit: 1m, productPrice: 50m);
        var submitResponse = await Client.PostAsJsonAsync("/api/requisitions",
            new SubmitRequisitionRequest { CartId = cart.Id, UserId = user.Id });
        var submitted = await submitResponse.Content.ReadFromJsonAsync<RequisitionDto>();

        var response = await Client.PatchAsync(
            $"/api/requisitions/{submitted!.Id}/approve?requestingUserRole=Buyer", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectRequisition_ByApprover_ReturnsRejected()
    {
        var (user, customer, cart) = await CreateUserCartWithItemAsync(budgetLimit: 1m, productPrice: 50m);
        var submitResponse = await Client.PostAsJsonAsync("/api/requisitions",
            new SubmitRequisitionRequest { CartId = cart.Id, UserId = user.Id });
        var submitted = await submitResponse.Content.ReadFromJsonAsync<RequisitionDto>();

        var response = await Client.PatchAsJsonAsync(
            $"/api/requisitions/{submitted!.Id}/reject?requestingUserRole=Approver",
            new RejectRequisitionRequest { Reason = "Over department budget" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rejected = await response.Content.ReadFromJsonAsync<RequisitionDto>();
        rejected!.Status.Should().Be("Rejected");
        rejected.RejectionReason.Should().Be("Over department budget");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string Uid() => Guid.NewGuid().ToString("N")[..8];

    private async Task<(UserDto user, CustomerDto customer, CartDto cart)> CreateUserCartWithItemAsync(
        decimal? budgetLimit, decimal productPrice)
    {
        var suffix = Uid();
        var customer = await CreateCustomerAsync($"REQ-{suffix}", $"Req Customer {suffix}", $"req-{suffix}@test.com");
        var user = await CreateUserAsync(customer.Id, $"buyer-{suffix}@test.com", "Buyer", "Test", budgetLimit);
        var product = await CreateProductAsync($"REQ-P-{suffix}".ToUpperInvariant()[..12], $"Req Product {suffix}", productPrice);

        var cartResponse = await Client.PostAsJsonAsync("/api/carts",
            new CreateCartRequest { UserId = user.Id, CustomerId = customer.Id });
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();

        await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 1 });

        var updatedCart = await Client.GetAsync($"/api/carts/{cart.Id}");
        cart = (await updatedCart.Content.ReadFromJsonAsync<CartDto>())!;

        return (user, customer, cart);
    }

    private async Task<CustomerDto> CreateCustomerAsync(string code, string name, string email)
    {
        var response = await Client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest { Code = code, Name = name, Email = email });
        return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
    }

    private async Task<UserDto> CreateUserAsync(Guid customerId, string email, string firstName, string lastName,
        decimal? budgetLimit = null)
    {
        var response = await Client.PostAsJsonAsync($"/api/customers/{customerId}/users",
            new CreateUserRequest
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = "Buyer",
                BudgetLimit = budgetLimit
            });
        return (await response.Content.ReadFromJsonAsync<UserDto>())!;
    }

    private async Task<ProductDto> CreateProductAsync(string sku, string name, decimal price)
    {
        var response = await Client.PostAsJsonAsync("/api/products",
            new CreateProductRequest { Sku = sku, Name = name, Price = price });
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }
}
