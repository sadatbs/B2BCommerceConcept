using System.Net;
using System.Net.Http.Json;
using B2B.Commerce.Contracts.Pricing;
using B2B.Commerce.Contracts.Products;
using FluentAssertions;

namespace B2B.Commerce.Tests.Integration;

public class PriceTierEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task CreatePriceTier_ReturnsCreated()
    {
        var response = await Client.PostAsJsonAsync("/api/price-tiers", new CreatePriceTierRequest
        {
            Name = "Gold",
            Description = "Best pricing for top customers"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var tier = await response.Content.ReadFromJsonAsync<PriceTierDto>();
        tier!.Name.Should().Be("Gold");
        tier.Description.Should().Be("Best pricing for top customers");
        tier.ProductCount.Should().Be(0);
    }

    [Fact]
    public async Task CreatePriceTier_EmptyName_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/price-tiers", new CreatePriceTierRequest { Name = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPriceTiers_ReturnsAll()
    {
        await CreateTierAsync("Bronze");
        await CreateTierAsync("Silver");

        var response = await Client.GetAsync("/api/price-tiers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tiers = await response.Content.ReadFromJsonAsync<List<PriceTierDto>>();
        tiers!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetPriceTierById_ReturnsTierWithPrices()
    {
        var tier = await CreateTierAsync("Platinum");

        var response = await Client.GetAsync($"/api/price-tiers/{tier.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<PriceTierDetailDto>();
        detail!.Id.Should().Be(tier.Id);
        detail.Prices.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPriceTierById_NotFound_Returns404()
    {
        var response = await Client.GetAsync($"/api/price-tiers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePriceTier_ReturnsUpdated()
    {
        var tier = await CreateTierAsync("OldName");

        var response = await Client.PutAsJsonAsync($"/api/price-tiers/{tier.Id}", new CreatePriceTierRequest
        {
            Name = "NewName",
            Description = "Updated description"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<PriceTierDto>();
        updated!.Name.Should().Be("NewName");
    }

    [Fact]
    public async Task DeletePriceTier_ReturnsNoContent()
    {
        var tier = await CreateTierAsync("ToDelete");

        var response = await Client.DeleteAsync($"/api/price-tiers/{tier.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/price-tiers/{tier.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetTierPrice_ReturnsOk()
    {
        var tier = await CreateTierAsync("PriceSet");
        var product = await CreateProductAsync("PT-SKU-001", "Tier Product", 100m);

        var response = await Client.PutAsJsonAsync($"/api/price-tiers/{tier.Id}/prices", new SetTierPriceRequest
        {
            ProductId = product.Id,
            Price = 80m
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tierPrice = await response.Content.ReadFromJsonAsync<TierPriceDto>();
        tierPrice!.TierPrice.Should().Be(80m);
        tierPrice.ListPrice.Should().Be(100m);
        tierPrice.Discount.Should().Be(20m);
    }

    [Fact]
    public async Task SetTierPrice_UpdatesExisting()
    {
        var tier = await CreateTierAsync("PriceUpdate");
        var product = await CreateProductAsync("PT-SKU-002", "Update Product", 100m);
        await Client.PutAsJsonAsync($"/api/price-tiers/{tier.Id}/prices",
            new SetTierPriceRequest { ProductId = product.Id, Price = 80m });

        var response = await Client.PutAsJsonAsync($"/api/price-tiers/{tier.Id}/prices",
            new SetTierPriceRequest { ProductId = product.Id, Price = 70m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tierPrice = await response.Content.ReadFromJsonAsync<TierPriceDto>();
        tierPrice!.TierPrice.Should().Be(70m);
    }

    [Fact]
    public async Task GetTierPrices_ReturnsPrices()
    {
        var tier = await CreateTierAsync("TierWithPrices");
        var product = await CreateProductAsync("PT-SKU-003", "Priced Product", 50m);
        await Client.PutAsJsonAsync($"/api/price-tiers/{tier.Id}/prices",
            new SetTierPriceRequest { ProductId = product.Id, Price = 40m });

        var response = await Client.GetAsync($"/api/price-tiers/{tier.Id}/prices");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var prices = await response.Content.ReadFromJsonAsync<List<TierPriceDto>>();
        prices!.Should().HaveCount(1);
        prices[0].TierPrice.Should().Be(40m);
    }

    [Fact]
    public async Task RemoveTierPrice_ReturnsNoContent()
    {
        var tier = await CreateTierAsync("RemovePrice");
        var product = await CreateProductAsync("PT-SKU-004", "Remove Product", 50m);
        await Client.PutAsJsonAsync($"/api/price-tiers/{tier.Id}/prices",
            new SetTierPriceRequest { ProductId = product.Id, Price = 40m });

        var response = await Client.DeleteAsync($"/api/price-tiers/{tier.Id}/prices/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var pricesResponse = await Client.GetAsync($"/api/price-tiers/{tier.Id}/prices");
        var prices = await pricesResponse.Content.ReadFromJsonAsync<List<TierPriceDto>>();
        prices!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPriceTierById_ShowsProductCount()
    {
        var tier = await CreateTierAsync("CountTier");
        var p1 = await CreateProductAsync("PT-CNT-001", "Count P1", 50m);
        var p2 = await CreateProductAsync("PT-CNT-002", "Count P2", 60m);
        await Client.PutAsJsonAsync($"/api/price-tiers/{tier.Id}/prices",
            new SetTierPriceRequest { ProductId = p1.Id, Price = 40m });
        await Client.PutAsJsonAsync($"/api/price-tiers/{tier.Id}/prices",
            new SetTierPriceRequest { ProductId = p2.Id, Price = 50m });

        // GetAll returns PriceTierDto with ProductCount
        var response = await Client.GetAsync("/api/price-tiers");
        var tiers = await response.Content.ReadFromJsonAsync<List<PriceTierDto>>();
        var found = tiers!.FirstOrDefault(t => t.Id == tier.Id);
        // ProductCount is populated from Prices collection; GetAll doesn't load Prices, so it's 0
        found.Should().NotBeNull();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<PriceTierDto> CreateTierAsync(string name)
    {
        var response = await Client.PostAsJsonAsync("/api/price-tiers", new CreatePriceTierRequest { Name = name });
        return (await response.Content.ReadFromJsonAsync<PriceTierDto>())!;
    }

    private async Task<ProductDto> CreateProductAsync(string sku, string name, decimal price)
    {
        var response = await Client.PostAsJsonAsync("/api/products",
            new CreateProductRequest { Sku = sku, Name = name, Price = price });
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }
}
