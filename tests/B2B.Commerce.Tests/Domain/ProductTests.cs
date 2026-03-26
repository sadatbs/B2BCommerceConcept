using B2B.Commerce.Domain.Entities;
using FluentAssertions;

namespace B2B.Commerce.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        var product = Product.Create("SKU-001", "Test Product", 99.99m);

        product.Id.Should().NotBeEmpty();
        product.Sku.Should().Be("SKU-001");
        product.Name.Should().Be("Test Product");
        product.Price.Should().Be(99.99m);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithEmptySku_ShouldThrowArgumentException()
    {
        var act = () => Product.Create("", "Test Product", 99.99m);

        act.Should().Throw<ArgumentException>()
           .WithParameterName("sku");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowArgumentException()
    {
        var act = () => Product.Create("SKU-001", "Test Product", -10m);

        act.Should().Throw<ArgumentException>()
           .WithParameterName("price");
    }

    [Fact]
    public void Create_ShouldNormalizeSku_ToUpperCase()
    {
        var product = Product.Create("sku-lowercase", "Test", 10m);

        product.Sku.Should().Be("SKU-LOWERCASE");
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldUpdatePriceAndTimestamp()
    {
        var product = Product.Create("SKU-001", "Test", 100m);

        product.UpdatePrice(150m);

        product.Price.Should().Be(150m);
        product.UpdatedAt.Should().NotBeNull();
    }
}
