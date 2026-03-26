using System.Net;
using System.Net.Http.Json;
using B2B.Commerce.Contracts.Catalogs;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Contracts.Products;
using FluentAssertions;

namespace B2B.Commerce.Tests.Integration;

public class CatalogEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateCatalog_WithValidData_ReturnsCreated()
    {
        var request = new CreateCatalogRequest { Name = "Test Catalog", Description = "Test Description" };

        var response = await Client.PostAsJsonAsync("/api/catalogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var catalog = await response.Content.ReadFromJsonAsync<CatalogDto>();
        catalog!.Name.Should().Be("Test Catalog");
        catalog.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateCatalog_WithEmptyName_ReturnsBadRequest()
    {
        var request = new CreateCatalogRequest { Name = "" };

        var response = await Client.PostAsJsonAsync("/api/catalogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Code.Should().Be("VALIDATION_FAILED");
    }

    [Fact]
    public async Task GetCatalogById_WithProducts_ReturnsCatalogDetail()
    {
        var catalog = await CreateCatalogAsync("Detail Catalog");
        var product = await CreateProductAsync("DETAIL-001", "Detail Product", 50m);

        await Client.PostAsJsonAsync($"/api/catalogs/{catalog.Id}/products",
            new AddProductsToCatalogRequest { ProductIds = new List<Guid> { product.Id } });

        var response = await Client.GetAsync($"/api/catalogs/{catalog.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<CatalogDetailDto>();
        detail!.Products.Should().HaveCount(1);
        detail.Products[0].Sku.Should().Be("DETAIL-001");
    }

    [Fact]
    public async Task AddProductsToCatalog_ShouldSucceed()
    {
        var catalog = await CreateCatalogAsync("Product Catalog");
        var product1 = await CreateProductAsync("PROD-001", "Product 1", 10m);
        var product2 = await CreateProductAsync("PROD-002", "Product 2", 20m);

        var addRequest = new AddProductsToCatalogRequest
        {
            ProductIds = new List<Guid> { product1.Id, product2.Id }
        };

        var response = await Client.PostAsJsonAsync($"/api/catalogs/{catalog.Id}/products", addRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var addedCount = await response.Content.ReadFromJsonAsync<int>();
        addedCount.Should().Be(2);
    }

    [Fact]
    public async Task AddProductsToCatalog_DuplicatesSkipped_NotError()
    {
        var catalog = await CreateCatalogAsync("Dedup Catalog");
        var product = await CreateProductAsync("DEDUP-001", "Dedup Product", 10m);

        var addRequest = new AddProductsToCatalogRequest { ProductIds = new List<Guid> { product.Id } };
        await Client.PostAsJsonAsync($"/api/catalogs/{catalog.Id}/products", addRequest);

        // Add again — should succeed, returning 0 added
        var response = await Client.PostAsJsonAsync($"/api/catalogs/{catalog.Id}/products", addRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var addedCount = await response.Content.ReadFromJsonAsync<int>();
        addedCount.Should().Be(0);
    }

    [Fact]
    public async Task GetCatalogProducts_ReturnsPaginatedList()
    {
        var catalog = await CreateCatalogAsync("Paged Catalog");
        for (int i = 1; i <= 5; i++)
        {
            var product = await CreateProductAsync($"PCAT-{i:D3}", $"Paged Product {i}", i * 10m);
            await Client.PostAsJsonAsync($"/api/catalogs/{catalog.Id}/products",
                new AddProductsToCatalogRequest { ProductIds = new List<Guid> { product.Id } });
        }

        var response = await Client.GetAsync($"/api/catalogs/{catalog.Id}/products?page=1&pageSize=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ProductDto>>();
        result!.Items.Should().HaveCountLessOrEqualTo(3);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task RemoveProductFromCatalog_ShouldSucceed()
    {
        var catalog = await CreateCatalogAsync("Remove Catalog");
        var product = await CreateProductAsync("REMOVE-001", "Remove Product", 10m);

        await Client.PostAsJsonAsync($"/api/catalogs/{catalog.Id}/products",
            new AddProductsToCatalogRequest { ProductIds = new List<Guid> { product.Id } });

        var response = await Client.DeleteAsync($"/api/catalogs/{catalog.Id}/products/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateCatalog_WithValidData_ReturnsUpdated()
    {
        var catalog = await CreateCatalogAsync("Original Name");

        var updateRequest = new UpdateCatalogRequest { Name = "Updated Name", Description = "New Desc" };
        var response = await Client.PutAsJsonAsync($"/api/catalogs/{catalog.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CatalogDto>();
        updated!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteCatalog_WhenExists_ReturnsNoContent()
    {
        var catalog = await CreateCatalogAsync("Delete Me");

        var response = await Client.DeleteAsync($"/api/catalogs/{catalog.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/catalogs/{catalog.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidSku_ReturnsBadRequest()
    {
        var request = new CreateProductRequest { Sku = "invalid sku!", Name = "Test", Price = 10m };

        var response = await Client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Code.Should().Be("VALIDATION_FAILED");
        error.Errors.Should().ContainKey("Sku");
    }

    // Helpers
    private async Task<CatalogDto> CreateCatalogAsync(string name)
    {
        var response = await Client.PostAsJsonAsync("/api/catalogs", new CreateCatalogRequest { Name = name });
        return (await response.Content.ReadFromJsonAsync<CatalogDto>())!;
    }

    private async Task<ProductDto> CreateProductAsync(string sku, string name, decimal price)
    {
        var response = await Client.PostAsJsonAsync("/api/products",
            new CreateProductRequest { Sku = sku, Name = name, Price = price });
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }
}
