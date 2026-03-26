using System.Net;
using System.Net.Http.Json;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Contracts.Products;
using FluentAssertions;

namespace B2B.Commerce.Tests.Integration;

public class ProductEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        var request = new CreateProductRequest
        {
            Sku = "TEST-001",
            Name = "Test Product",
            Description = "A test product",
            Price = 99.99m,
            Category = "Electronics"
        };

        var response = await Client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.Sku.Should().Be("TEST-001");
        product.Name.Should().Be("Test Product");
        product.Price.Should().Be(99.99m);
        product.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_ReturnsConflict()
    {
        var request = new CreateProductRequest { Sku = "DUPLICATE-SKU", Name = "First Product", Price = 50m };
        await Client.PostAsJsonAsync("/api/products", request);

        var duplicateRequest = new CreateProductRequest { Sku = "DUPLICATE-SKU", Name = "Second Product", Price = 75m };

        var response = await Client.PostAsJsonAsync("/api/products", duplicateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetProductById_WhenExists_ReturnsProduct()
    {
        var createRequest = new CreateProductRequest { Sku = "GET-BY-ID-TEST", Name = "Get By Id Test", Price = 100m };
        var createResponse = await Client.PostAsJsonAsync("/api/products", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var response = await Client.GetAsync($"/api/products/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetProductById_WhenNotExists_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductBySku_WhenExists_ReturnsProduct()
    {
        var createRequest = new CreateProductRequest { Sku = "SKU-LOOKUP-TEST", Name = "SKU Lookup Test", Price = 150m };
        await Client.PostAsJsonAsync("/api/products", createRequest);

        var response = await Client.GetAsync("/api/products/sku/SKU-LOOKUP-TEST");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product!.Sku.Should().Be("SKU-LOOKUP-TEST");
    }

    [Fact]
    public async Task GetProducts_ReturnsPaginatedList()
    {
        for (int i = 1; i <= 5; i++)
        {
            await Client.PostAsJsonAsync("/api/products", new CreateProductRequest
            {
                Sku = $"PAGED-{i:D3}",
                Name = $"Paged Product {i}",
                Price = i * 10m
            });
        }

        var response = await Client.GetAsync("/api/products?page=1&pageSize=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResponse<ProductDto>>();
        pagedResult!.PageSize.Should().Be(3);
        pagedResult.Items.Should().HaveCountLessOrEqualTo(3);
        pagedResult.TotalCount.Should().BeGreaterOrEqualTo(5);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsUpdatedProduct()
    {
        var createRequest = new CreateProductRequest { Sku = "UPDATE-TEST", Name = "Original Name", Price = 100m };
        var createResponse = await Client.PostAsJsonAsync("/api/products", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var updateRequest = new UpdateProductRequest
        {
            Name = "Updated Name",
            Description = "New description",
            Category = "Updated Category"
        };

        var response = await Client.PutAsJsonAsync($"/api/products/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>();
        updated!.Name.Should().Be("Updated Name");
        updated.Description.Should().Be("New description");
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateProductPrice_WithValidPrice_ReturnsUpdatedProduct()
    {
        var createRequest = new CreateProductRequest { Sku = "PRICE-UPDATE-TEST", Name = "Price Test", Price = 100m };
        var createResponse = await Client.PostAsJsonAsync("/api/products", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var response = await Client.PatchAsJsonAsync($"/api/products/{created!.Id}/price", new UpdateProductPriceRequest { Price = 199.99m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>();
        updated!.Price.Should().Be(199.99m);
    }

    [Fact]
    public async Task DeleteProduct_WhenExists_ReturnsNoContent()
    {
        var createRequest = new CreateProductRequest { Sku = "DELETE-TEST", Name = "Delete Test", Price = 50m };
        var createResponse = await Client.PostAsJsonAsync("/api/products", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var response = await Client.DeleteAsync($"/api/products/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/products/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
