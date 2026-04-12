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

public class OrderEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task ConfirmOrder_FromPending_Succeeds()
    {
        var order = await CreateOrderAsync();

        var response = await Client.PostAsync($"/api/orders/{order.Id}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var confirmedOrder = await response.Content.ReadFromJsonAsync<OrderDto>();
        confirmedOrder!.Status.Should().Be("Confirmed");
        confirmedOrder.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteOrder_FromConfirmed_Succeeds()
    {
        var order = await CreateOrderAsync();
        await Client.PostAsync($"/api/orders/{order.Id}/confirm", null);

        var response = await Client.PostAsync($"/api/orders/{order.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var completedOrder = await response.Content.ReadFromJsonAsync<OrderDto>();
        completedOrder!.Status.Should().Be("Completed");
        completedOrder.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ConfirmOrder_AlreadyConfirmed_ReturnsBadRequest()
    {
        var order = await CreateOrderAsync();
        await Client.PostAsync($"/api/orders/{order.Id}/confirm", null);

        var response = await Client.PostAsync($"/api/orders/{order.Id}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrderById_ReturnsOrderWithItems()
    {
        var order = await CreateOrderAsync();

        var response = await Client.GetAsync($"/api/orders/{order.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<OrderDetailDto>();
        detail!.Items.Should().NotBeEmpty();
        detail.Items[0].Sku.Should().NotBeEmpty();
        detail.Items[0].ProductName.Should().NotBeEmpty();
        detail.Items[0].UnitPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetOrders_ReturnsPaginatedList()
    {
        await CreateOrderAsync();

        var response = await Client.GetAsync("/api/orders?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates an order via the full requisition workflow:
    /// Customer → User (no budget limit → auto-approve) → Cart → Item → Submit Requisition → Order
    /// </summary>
    private async Task<OrderDto> CreateOrderAsync()
    {
        // Create customer + user with no budget limit (triggers auto-approve)
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = await CreateCustomerAsync($"ORD-{suffix}", $"Order Customer {suffix}", $"order-{suffix}@test.com");
        var user = await CreateUserAsync(customer.Id, $"buyer-{suffix}@test.com", "Buyer", "Test");

        // Create cart and add a product
        var product = await CreateProductAsync($"ORD-P-{suffix}".ToUpperInvariant()[..12], $"Order Product {suffix}", 50.00m);
        var cartResponse = await Client.PostAsJsonAsync("/api/carts",
            new CreateCartRequest { UserId = user.Id, CustomerId = customer.Id });
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();

        await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 1 });

        // Submit requisition → auto-approved → order created
        var reqResponse = await Client.PostAsJsonAsync("/api/requisitions",
            new SubmitRequisitionRequest { CartId = cart.Id, UserId = user.Id });
        reqResponse.EnsureSuccessStatusCode();

        // Fetch the newly created order (only 1 order exists per test isolation)
        var ordersResponse = await Client.GetAsync("/api/orders?page=1&pageSize=100");
        var paged = await ordersResponse.Content.ReadFromJsonAsync<PagedResponse<OrderDto>>();
        return paged!.Items.Last();
    }

    private async Task<CustomerDto> CreateCustomerAsync(string code, string name, string email)
    {
        var response = await Client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest { Code = code, Name = name, Email = email });
        return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
    }

    private async Task<UserDto> CreateUserAsync(Guid customerId, string email, string firstName, string lastName)
    {
        var response = await Client.PostAsJsonAsync($"/api/customers/{customerId}/users",
            new CreateUserRequest { Email = email, FirstName = firstName, LastName = lastName, Role = "Buyer" });
        return (await response.Content.ReadFromJsonAsync<UserDto>())!;
    }

    private async Task<ProductDto> CreateProductAsync(string sku, string name, decimal price)
    {
        var request = new CreateProductRequest { Sku = sku, Name = name, Price = price };
        var response = await Client.PostAsJsonAsync("/api/products", request);
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }
}
