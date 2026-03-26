using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Infrastructure.Data;
using B2B.Commerce.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Tests.Infrastructure;

public class CatalogRepositoryTests
{
    private static CommerceDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new CommerceDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistCatalog()
    {
        await using var context = CreateInMemoryContext();
        var repository = new CatalogRepository(context);
        var catalog = Catalog.Create("Test Catalog", "Description");

        var result = await repository.AddAsync(catalog);

        result.Id.Should().NotBeEmpty();
        var persisted = await context.Catalogs.FindAsync(catalog.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsAsync_WhenExists_ShouldReturnTrue()
    {
        await using var context = CreateInMemoryContext();
        var repository = new CatalogRepository(context);
        var catalog = Catalog.Create("Test Catalog");
        await repository.AddAsync(catalog);

        var result = await repository.ExistsAsync(catalog.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenNotExists_ShouldReturnFalse()
    {
        await using var context = CreateInMemoryContext();
        var repository = new CatalogRepository(context);

        var result = await repository.ExistsAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddProductToCatalogAsync_ShouldCreateRelationship()
    {
        await using var context = CreateInMemoryContext();
        var catalogRepo = new CatalogRepository(context);
        var productRepo = new ProductRepository(context);

        var catalog = Catalog.Create("Test Catalog");
        var product = Product.Create("SKU-001", "Test Product", 99.99m);

        await catalogRepo.AddAsync(catalog);
        await productRepo.AddAsync(product);

        await catalogRepo.AddProductToCatalogAsync(catalog.Id, product.Id);

        var isInCatalog = await catalogRepo.IsProductInCatalogAsync(catalog.Id, product.Id);
        isInCatalog.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductsInCatalogAsync_ShouldReturnProducts()
    {
        await using var context = CreateInMemoryContext();
        var catalogRepo = new CatalogRepository(context);
        var productRepo = new ProductRepository(context);

        var catalog = Catalog.Create("Test Catalog");
        var product1 = Product.Create("SKU-001", "Product 1", 10m);
        var product2 = Product.Create("SKU-002", "Product 2", 20m);

        await catalogRepo.AddAsync(catalog);
        await productRepo.AddAsync(product1);
        await productRepo.AddAsync(product2);

        await catalogRepo.AddProductsToCatalogAsync(catalog.Id, new[] { product1.Id, product2.Id });

        var products = await catalogRepo.GetProductsInCatalogAsync(catalog.Id);

        products.Should().HaveCount(2);
    }

    [Fact]
    public async Task RemoveProductFromCatalogAsync_ShouldRemoveRelationship()
    {
        await using var context = CreateInMemoryContext();
        var catalogRepo = new CatalogRepository(context);
        var productRepo = new ProductRepository(context);

        var catalog = Catalog.Create("Test Catalog");
        var product = Product.Create("SKU-001", "Test Product", 99.99m);

        await catalogRepo.AddAsync(catalog);
        await productRepo.AddAsync(product);
        await catalogRepo.AddProductToCatalogAsync(catalog.Id, product.Id);

        await catalogRepo.RemoveProductFromCatalogAsync(catalog.Id, product.Id);

        var isInCatalog = await catalogRepo.IsProductInCatalogAsync(catalog.Id, product.Id);
        isInCatalog.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        await using var context = CreateInMemoryContext();
        var repository = new CatalogRepository(context);
        var catalog = Catalog.Create("Original Name", "Original Desc");
        await repository.AddAsync(catalog);

        catalog.UpdateDetails("New Name", "New Desc");
        await repository.UpdateAsync(catalog);

        var updated = await repository.GetByIdAsync(catalog.Id);
        updated!.Name.Should().Be("New Name");
        updated.Description.Should().Be("New Desc");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCatalog()
    {
        await using var context = CreateInMemoryContext();
        var repository = new CatalogRepository(context);
        var catalog = Catalog.Create("To Delete");
        await repository.AddAsync(catalog);

        await repository.DeleteAsync(catalog.Id);

        var result = await repository.GetByIdAsync(catalog.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPage()
    {
        await using var context = CreateInMemoryContext();
        var repository = new CatalogRepository(context);

        for (int i = 1; i <= 5; i++)
            await repository.AddAsync(Catalog.Create($"Catalog {i:D2}"));

        var (items, totalCount) = await repository.GetPagedAsync(skip: 0, take: 3);

        items.Should().HaveCount(3);
        totalCount.Should().Be(5);
    }
}
