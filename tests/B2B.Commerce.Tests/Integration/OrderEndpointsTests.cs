using System.Net;
using System.Net.Http.Json;
using B2B.Commerce.Contracts.Carts;
using B2B.Commerce.Contracts.Orders;
using B2B.Commerce.Contracts.Products;
using FluentAssertions;

namespace B2B.Commerce.Tests.Integration;

public class OrderEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateOrder_FromCartWithItems_ReturnsCreated()
    {
        var cart = await CreateCartWithItemsAsync(2);

        var request = new CreateOrderRequest
        {
            CartId = cart.Id,
            PurchaseOrderNumber = "PO-2026-001"
        };
        var response = await Client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order!.Status.Should().Be("Pending");
        order.PurchaseOrderNumber.Should().Be("PO-2026-001");
        order.ItemCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateOrder_ClearsCart()
    {
        var cart = await CreateCartWithItemsAsync(1);

        var request = new CreateOrderRequest { CartId = cart.Id };
        await Client.PostAsJsonAsync("/api/orders", request);

        var cartResponse = await Client.GetAsync($"/api/carts/{cart.Id}");
        var updatedCart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();
        updatedCart!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateOrder_EmptyCart_ReturnsBadRequest()
    {
        var cartResponse = await Client.PostAsJsonAsync("/api/carts", new CreateCartRequest());
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();

        var request = new CreateOrderRequest { CartId = cart!.Id };
        var response = await Client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

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

    private async Task<CartDto> CreateCartWithItemsAsync(int itemCount)
    {
        var cartResponse = await Client.PostAsJsonAsync("/api/carts", new CreateCartRequest());
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();

        for (int i = 1; i <= itemCount; i++)
        {
            var sku = $"ORD-{Guid.NewGuid():N}".Substring(0, 20).ToUpperInvariant();
            var product = await CreateProductAsync(sku, $"Order Product {i}", i * 50.00m);
            await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
                new AddToCartRequest { ProductId = product.Id, Quantity = i });
        }

        var updatedResponse = await Client.GetAsync($"/api/carts/{cart!.Id}");
        return (await updatedResponse.Content.ReadFromJsonAsync<CartDto>())!;
    }

    private async Task<OrderDto> CreateOrderAsync()
    {
        var cart = await CreateCartWithItemsAsync(1);
        var request = new CreateOrderRequest { CartId = cart.Id };
        var response = await Client.PostAsJsonAsync("/api/orders", request);
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    private async Task<ProductDto> CreateProductAsync(string sku, string name, decimal price)
    {
        var request = new CreateProductRequest { Sku = sku, Name = name, Price = price };
        var response = await Client.PostAsJsonAsync("/api/products", request);
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }
}
