using System.Net;
using System.Net.Http.Json;
using B2B.Commerce.Contracts.Carts;
using B2B.Commerce.Contracts.Products;
using FluentAssertions;

namespace B2B.Commerce.Tests.Integration;

public class CartEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateCart_ReturnsCreated()
    {
        var response = await Client.PostAsJsonAsync("/api/carts", new CreateCartRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var cart = await response.Content.ReadFromJsonAsync<CartDto>();
        cart!.Id.Should().NotBeEmpty();
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task AddToCart_ReturnsUpdatedCart()
    {
        var cartResponse = await Client.PostAsJsonAsync("/api/carts", new CreateCartRequest());
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();

        var product = await CreateProductAsync("CART-001", "Cart Test Product", 25.00m);

        var addRequest = new AddToCartRequest { ProductId = product.Id, Quantity = 3 };
        var response = await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items", addRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedCart = await response.Content.ReadFromJsonAsync<CartDto>();
        updatedCart!.Items.Should().HaveCount(1);
        updatedCart.Items[0].Quantity.Should().Be(3);
        updatedCart.Subtotal.Should().Be(75.00m);
    }

    [Fact]
    public async Task AddToCart_SameProductTwice_IncreasesQuantity()
    {
        var cartResponse = await Client.PostAsJsonAsync("/api/carts", new CreateCartRequest());
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();
        var product = await CreateProductAsync("CART-002", "Cart Test Product 2", 10.00m);

        await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 2 });
        var response = await Client.PostAsJsonAsync($"/api/carts/{cart.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 3 });

        var updatedCart = await response.Content.ReadFromJsonAsync<CartDto>();
        updatedCart!.Items.Should().HaveCount(1);
        updatedCart.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public async Task AddToCart_ZeroQuantity_ReturnsBadRequest()
    {
        var cartResponse = await Client.PostAsJsonAsync("/api/carts", new CreateCartRequest());
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();
        var product = await CreateProductAsync("CART-003", "Cart Test Product 3", 10.00m);

        var response = await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 0 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveFromCart_ReturnsUpdatedCart()
    {
        var cartResponse = await Client.PostAsJsonAsync("/api/carts", new CreateCartRequest());
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();
        var product = await CreateProductAsync("CART-004", "Remove Test", 15.00m);

        await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 1 });

        var response = await Client.DeleteAsync($"/api/carts/{cart.Id}/items/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedCart = await response.Content.ReadFromJsonAsync<CartDto>();
        updatedCart!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCart_ReturnsCartWithItems()
    {
        var cartResponse = await Client.PostAsJsonAsync("/api/carts", new CreateCartRequest());
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();
        var product = await CreateProductAsync("CART-005", "Get Test", 20.00m);

        await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 2 });

        var response = await Client.GetAsync($"/api/carts/{cart.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await response.Content.ReadFromJsonAsync<CartDto>();
        fetched!.Items.Should().HaveCount(1);
        fetched.Items[0].Sku.Should().Be("CART-005");
    }

    private async Task<ProductDto> CreateProductAsync(string sku, string name, decimal price)
    {
        var request = new CreateProductRequest { Sku = sku, Name = name, Price = price };
        var response = await Client.PostAsJsonAsync("/api/products", request);
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }
}
