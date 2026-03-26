using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Infrastructure.Data;
using B2B.Commerce.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace B2B.Commerce.Tests.Infrastructure;

public class ProductRepositoryTests
{
    private static CommerceDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new CommerceDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistProduct()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = Product.Create("TEST-SKU", "Test Product", 99.99m);

        var result = await repository.AddAsync(product);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        var persisted = await context.Products.FindAsync(product.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnProduct()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = Product.Create("TEST-SKU", "Test Product", 99.99m);
        await repository.AddAsync(product);

        var result = await repository.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Sku.Should().Be("TEST-SKU");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySkuAsync_ShouldBeCaseInsensitive()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = Product.Create("TEST-SKU", "Test Product", 99.99m);
        await repository.AddAsync(product);

        var result = await repository.GetBySkuAsync("test-sku");

        result.Should().NotBeNull();
        result!.Sku.Should().Be("TEST-SKU");
    }

    [Fact]
    public async Task SkuExistsAsync_WhenExists_ShouldReturnTrue()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = Product.Create("EXISTS-SKU", "Test Product", 50m);
        await repository.AddAsync(product);

        var result = await repository.SkuExistsAsync("exists-sku");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SkuExistsAsync_WhenNotExists_ShouldReturnFalse()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);

        var result = await repository.SkuExistsAsync("GHOST-SKU");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPage()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);

        for (int i = 1; i <= 10; i++)
        {
            await repository.AddAsync(Product.Create($"SKU-{i:D3}", $"Product {i}", i * 10m));
        }

        var (items, totalCount) = await repository.GetPagedAsync(skip: 0, take: 3);

        items.Should().HaveCount(3);
        totalCount.Should().Be(10);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = Product.Create("UPDATE-SKU", "Original Name", 100m);
        await repository.AddAsync(product);

        product.UpdateDetails("New Name", "New Description", "New Category");
        await repository.UpdateAsync(product);

        var updated = await repository.GetByIdAsync(product.Id);
        updated!.Name.Should().Be("New Name");
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProduct()
    {
        await using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = Product.Create("DELETE-SKU", "To Delete", 10m);
        await repository.AddAsync(product);

        await repository.DeleteAsync(product.Id);

        var result = await repository.GetByIdAsync(product.Id);
        result.Should().BeNull();
    }
}
