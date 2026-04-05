using B2B.Commerce.Domain.Entities;
using FluentAssertions;

namespace B2B.Commerce.Tests.Domain;

public class PriceTierTests
{
    [Fact]
    public void Create_WithValidName_CreatesTier()
    {
        var tier = PriceTier.Create("Gold");

        tier.Id.Should().NotBeEmpty();
        tier.Name.Should().Be("Gold");
        tier.Description.Should().BeNull();
        tier.Prices.Should().BeEmpty();
        tier.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithDescription_SetsDescription()
    {
        var tier = PriceTier.Create("Silver", "Mid-tier pricing");

        tier.Description.Should().Be("Mid-tier pricing");
    }

    [Fact]
    public void Create_TrimsName()
    {
        var tier = PriceTier.Create("  Gold  ");

        tier.Name.Should().Be("Gold");
    }

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        var act = () => PriceTier.Create("");

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void UpdateDetails_UpdatesNameAndDescription()
    {
        var tier = PriceTier.Create("Gold");

        tier.UpdateDetails("Platinum", "Top tier");

        tier.Name.Should().Be("Platinum");
        tier.Description.Should().Be("Top tier");
    }

    [Fact]
    public void UpdateDetails_WithNullDescription_ClearsDescription()
    {
        var tier = PriceTier.Create("Gold", "Some desc");

        tier.UpdateDetails("Gold", null);

        tier.Description.Should().BeNull();
    }
}

public class TierPriceTests
{
    [Fact]
    public void Create_WithValidData_CreatesTierPrice()
    {
        var tierId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var tierPrice = TierPrice.Create(tierId, productId, 79.99m);

        tierPrice.PriceTierId.Should().Be(tierId);
        tierPrice.ProductId.Should().Be(productId);
        tierPrice.Price.Should().Be(79.99m);
        tierPrice.UpdatedAt.Should().BeNull();
        tierPrice.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithZeroPrice_Succeeds()
    {
        var tierPrice = TierPrice.Create(Guid.NewGuid(), Guid.NewGuid(), 0m);

        tierPrice.Price.Should().Be(0m);
    }

    [Fact]
    public void Create_WithNegativePrice_Throws()
    {
        var act = () => TierPrice.Create(Guid.NewGuid(), Guid.NewGuid(), -1m);

        act.Should().Throw<ArgumentException>().WithParameterName("price");
    }

    [Fact]
    public void UpdatePrice_ChangesPrice()
    {
        var tierPrice = TierPrice.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);

        tierPrice.UpdatePrice(85m);

        tierPrice.Price.Should().Be(85m);
        tierPrice.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePrice_WithNegativeValue_Throws()
    {
        var tierPrice = TierPrice.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);

        var act = () => tierPrice.UpdatePrice(-5m);

        act.Should().Throw<ArgumentException>().WithParameterName("price");
    }
}
