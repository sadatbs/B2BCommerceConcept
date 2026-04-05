using System.Net;
using System.Net.Http.Json;
using B2B.Commerce.Contracts.Carts;
using B2B.Commerce.Contracts.Customers;
using B2B.Commerce.Contracts.Pricing;
using B2B.Commerce.Contracts.Products;
using FluentAssertions;

namespace B2B.Commerce.Tests.Integration;

public class CartPricingTests : IntegrationTestBase
{
    [Fact]
    public async Task Cart_WithoutCustomer_ShowsListPrice()
    {
        var product = await CreateProductAsync("CP-001", "Pricing Widget", 100m);
        var cartResponse = await Client.PostAsJsonAsync("/api/carts", new CreateCartRequest());
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();

        await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 1 });

        var getResponse = await Client.GetAsync($"/api/carts/{cart.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<CartDto>();

        fetched!.Items[0].UnitPrice.Should().Be(100m);
        fetched.Subtotal.Should().Be(100m);
    }

    [Fact]
    public async Task Cart_CustomerWithTier_ShowsTierPrice()
    {
        var product = await CreateProductAsync("CP-002", "Tier Widget", 100m);
        var tier = await CreatePriceTierAsync("Gold");
        await Client.PutAsJsonAsync($"/api/price-tiers/{tier.Id}/prices",
            new SetTierPriceRequest { ProductId = product.Id, Price = 75m });

        var customer = await CreateCustomerAsync("CP-CUST-001", "Tier Customer", "tier@customer.com");
        await Client.PutAsJsonAsync($"/api/customers/{customer.Id}/price-tier",
            new AssignPriceTierRequest { PriceTierId = tier.Id });

        var cartResponse = await Client.PostAsJsonAsync("/api/carts",
            new CreateCartRequest { CustomerId = customer.Id });
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();

        await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 2 });

        var getResponse = await Client.GetAsync($"/api/carts/{cart.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<CartDto>();

        fetched!.Items[0].UnitPrice.Should().Be(75m);
        fetched.Subtotal.Should().Be(150m);
    }

    [Fact]
    public async Task Cart_CustomerWithTier_ProductNotInTier_FallsBackToListPrice()
    {
        var product = await CreateProductAsync("CP-003", "Fallback Widget", 100m);
        var tier = await CreatePriceTierAsync("Silver");
        // No tier price set for this product

        var customer = await CreateCustomerAsync("CP-CUST-002", "Silver Customer", "silver@customer.com");
        await Client.PutAsJsonAsync($"/api/customers/{customer.Id}/price-tier",
            new AssignPriceTierRequest { PriceTierId = tier.Id });

        var cartResponse = await Client.PostAsJsonAsync("/api/carts",
            new CreateCartRequest { CustomerId = customer.Id });
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();

        await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 1 });

        var getResponse = await Client.GetAsync($"/api/carts/{cart.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<CartDto>();

        fetched!.Items[0].UnitPrice.Should().Be(100m);
    }

    [Fact]
    public async Task Cart_CustomerWithoutTier_ShowsListPrice()
    {
        var product = await CreateProductAsync("CP-004", "NoTier Widget", 50m);
        var customer = await CreateCustomerAsync("CP-CUST-003", "No Tier Customer", "notier@customer.com");
        // No tier assigned

        var cartResponse = await Client.PostAsJsonAsync("/api/carts",
            new CreateCartRequest { CustomerId = customer.Id });
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();

        await Client.PostAsJsonAsync($"/api/carts/{cart!.Id}/items",
            new AddToCartRequest { ProductId = product.Id, Quantity = 3 });

        var getResponse = await Client.GetAsync($"/api/carts/{cart.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<CartDto>();

        fetched!.Items[0].UnitPrice.Should().Be(50m);
        fetched.Subtotal.Should().Be(150m);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<ProductDto> CreateProductAsync(string sku, string name, decimal price)
    {
        var response = await Client.PostAsJsonAsync("/api/products",
            new CreateProductRequest { Sku = sku, Name = name, Price = price });
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }

    private async Task<PriceTierDto> CreatePriceTierAsync(string name)
    {
        var response = await Client.PostAsJsonAsync("/api/price-tiers", new CreatePriceTierRequest { Name = name });
        return (await response.Content.ReadFromJsonAsync<PriceTierDto>())!;
    }

    private async Task<CustomerDto> CreateCustomerAsync(string code, string name, string email)
    {
        var response = await Client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest { Code = code, Name = name, Email = email });
        return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
    }
}
